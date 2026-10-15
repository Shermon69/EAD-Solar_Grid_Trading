/*
 * File:        MapActivity.java
 * Author:      WMVSB Wahundeniya (IT22292872)
 * Description: Shows nearby solar stations on a map. The phone's location is
 *              sent to GET /api/stations/nearby, which returns the active
 *              stations inside the chosen radius. Each station is plotted from its
 *              stored latitude and longitude; tapping one shows a summary card with
 *              Directions and View details. The map is Google Maps when a Maps key
 *              is available, or the free OpenStreetMap fallback otherwise, and can
 *              be switched with the layers button. Stations are cached in SQLite so
 *              the map still works offline.
 * Created:     29/09/2026
 */

package com.solargrid.app.activities;

import android.Manifest;
import android.annotation.SuppressLint;
import android.content.ActivityNotFoundException;
import android.content.Context;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.location.Location;
import android.net.Uri;
import android.os.Bundle;
import android.view.View;
import android.widget.TextView;
import android.widget.Toast;

import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.contract.ActivityResultContracts;
import androidx.annotation.NonNull;
import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.content.ContextCompat;

import com.google.android.gms.location.FusedLocationProviderClient;
import com.google.android.gms.location.LocationServices;
import com.google.android.gms.location.Priority;
import com.google.android.gms.tasks.CancellationTokenSource;
import com.google.android.material.card.MaterialCardView;
import com.google.android.material.chip.Chip;
import com.google.android.material.chip.ChipGroup;
import com.google.android.material.progressindicator.LinearProgressIndicator;
import com.solargrid.app.R;
import com.solargrid.app.api.ApiClient;
import com.solargrid.app.db.DatabaseHelper;
import com.solargrid.app.db.StationDao;
import com.solargrid.app.maps.StationMap;
import com.solargrid.app.maps.StationMaps;
import com.solargrid.app.models.Station;

import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;
import java.util.List;
import java.util.Locale;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

/**
 * Map of nearby solar stations (prosumers and grid operators).
 */
public class MapActivity extends AppCompatActivity implements StationMap.Listener {

    // Search radius options in km. 0 means "All" active stations
    private static final double[] RADIUS_OPTIONS_KM = {5, 10, 25, 50, 0};
    private static final int DEFAULT_RADIUS_INDEX = 2;

    // Camera position used before the phone's location or any station is known (centre of Sri Lanka)
    private static final double DEFAULT_LATITUDE = 7.8731;
    private static final double DEFAULT_LONGITUDE = 80.7718;

    private StationMap stationMap;
    private boolean mapReady = false;
    private StationDao stationDao;
    private FusedLocationProviderClient locationClient;

    private Location userLocation;
    private double radiusKm = RADIUS_OPTIONS_KM[DEFAULT_RADIUS_INDEX];
    private final List<Station> stations = new ArrayList<>();
    private Station selectedStation;

    private View layoutTopOverlay;
    private View layoutBottomOverlay;
    private TextView tvMapSubtitle;
    private TextView tvOfflineBanner;
    private LinearProgressIndicator progressMap;
    private MaterialCardView cardStation;
    private TextView tvCardName;
    private TextView tvCardAddress;
    private TextView tvCardDistance;
    private TextView tvCardCapacity;
    private TextView tvCardSlots;

    // Asks for location permission. Whatever the answer, the stations are then loaded
    private final ActivityResultLauncher<String[]> locationPermissionLauncher =
            registerForActivityResult(new ActivityResultContracts.RequestMultiplePermissions(), result -> {
                if (hasLocationPermission()) {
                    stationMap.setMyLocationEnabled(true);
                    locateUserThenLoad();
                } else {
                    Toast.makeText(this, R.string.map_location_denied, Toast.LENGTH_LONG).show();
                    loadStations();
                }
            });

    /**
     * Links the views, builds the radius chips and adds the chosen map.
     */
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_map);

        stationDao = new StationDao(DatabaseHelper.getInstance(this));
        locationClient = LocationServices.getFusedLocationProviderClient(this);

        layoutTopOverlay = findViewById(R.id.layoutTopOverlay);
        layoutBottomOverlay = findViewById(R.id.layoutBottomOverlay);
        tvMapSubtitle = findViewById(R.id.tvMapSubtitle);
        tvOfflineBanner = findViewById(R.id.tvOfflineBanner);
        progressMap = findViewById(R.id.progressMap);
        cardStation = findViewById(R.id.cardStation);
        tvCardName = findViewById(R.id.tvCardName);
        tvCardAddress = findViewById(R.id.tvCardAddress);
        tvCardDistance = findViewById(R.id.tvCardDistance);
        tvCardCapacity = findViewById(R.id.tvCardCapacity);
        tvCardSlots = findViewById(R.id.tvCardSlots);

        findViewById(R.id.btnBack).setOnClickListener(v -> finish());
        findViewById(R.id.btnRefresh).setOnClickListener(v -> refresh());
        findViewById(R.id.btnMapType).setOnClickListener(v -> chooseMapType());
        findViewById(R.id.fabMyLocation).setOnClickListener(v -> onMyLocationClicked());
        findViewById(R.id.btnCardDirections).setOnClickListener(v -> openDirections(this, selectedStation));
        findViewById(R.id.btnCardDetails).setOnClickListener(v -> openStationDetails(selectedStation));

        setupRadiusChips();

        stationMap = StationMaps.create(this);
        stationMap.setListener(this);
        stationMap.attach(this, findViewById(R.id.mapContainer), this::onMapReady);
    }

    /**
     * Called once the map can be used. Finds the user's location (asking for
     * permission if needed) and then loads the stations.
     */
    private void onMapReady() {
        mapReady = true;
        stationMap.moveTo(DEFAULT_LATITUDE, DEFAULT_LONGITUDE, 7, false);

        // Keep map controls and camera moves clear of the floating top bar and bottom card
        layoutTopOverlay.addOnLayoutChangeListener((v, l, t, r, b, ol, ot, or, ob) -> updateMapPadding());
        layoutBottomOverlay.addOnLayoutChangeListener((v, l, t, r, b, ol, ot, or, ob) -> updateMapPadding());
        updateMapPadding();

        if (hasLocationPermission()) {
            stationMap.setMyLocationEnabled(true);
            locateUserThenLoad();
        } else {
            locationPermissionLauncher.launch(new String[]{
                    Manifest.permission.ACCESS_FINE_LOCATION,
                    Manifest.permission.ACCESS_COARSE_LOCATION});
        }
    }

    /**
     * Forwards onResume to the map (OpenStreetMap needs it to load tiles and track GPS).
     */
    @Override
    protected void onResume() {
        super.onResume();
        stationMap.onResume();
    }

    /**
     * Forwards onPause to the map so it stops using GPS and the network.
     */
    @Override
    protected void onPause() {
        stationMap.onPause();
        super.onPause();
    }

    /**
     * Releases the map's resources.
     */
    @Override
    protected void onDestroy() {
        stationMap.onDestroy();
        super.onDestroy();
    }

    /**
     * A marker was tapped: show its summary card.
     */
    @Override
    public void onStationTapped(Station station) {
        selectStation(station);
    }

    /**
     * Google's info bubble was tapped: open the station details.
     */
    @Override
    public void onStationDetailsTapped(Station station) {
        openStationDetails(station);
    }

    /**
     * An empty part of the map was tapped: hide the summary card.
     */
    @Override
    public void onMapTapped() {
        hideStationCard();
    }

    /**
     * Lets the user switch between Google Maps and OpenStreetMap. The screen is
     * recreated so the new map replaces the old one.
     */
    private void chooseMapType() {
        String[] labels = {getString(R.string.map_type_google), getString(R.string.map_type_osm)};
        String[] values = {StationMaps.PROVIDER_GOOGLE, StationMaps.PROVIDER_OSM};
        int current = StationMaps.PROVIDER_GOOGLE.equals(StationMaps.getProvider(this)) ? 0 : 1;

        new AlertDialog.Builder(this)
                .setTitle(R.string.map_type)
                .setSingleChoiceItems(labels, current, (dialog, which) -> {
                    dialog.dismiss();
                    if (which == current) {
                        return;
                    }
                    if (StationMaps.PROVIDER_GOOGLE.equals(values[which]) && !StationMaps.hasGoogleKey(this)) {
                        Toast.makeText(this, R.string.map_type_no_key, Toast.LENGTH_LONG).show();
                        return;
                    }
                    StationMaps.setProvider(this, values[which]);
                    recreate();
                })
                .show();
    }

    /**
     * Adds one chip per radius option (5 km ... All) and reloads the
     * stations whenever a different radius is picked.
     */
    private void setupRadiusChips() {
        ChipGroup group = findViewById(R.id.chipGroupRadius);
        for (int i = 0; i < RADIUS_OPTIONS_KM.length; i++) {
            double km = RADIUS_OPTIONS_KM[i];
            Chip chip = new Chip(this);
            chip.setId(View.generateViewId());
            chip.setTag(km);
            chip.setText(km == 0 ? getString(R.string.map_radius_all)
                    : String.format(Locale.ENGLISH, "%.0f km", km));
            chip.setCheckable(true);
            chip.setCheckedIconVisible(false);
            chip.setChipBackgroundColor(ContextCompat.getColorStateList(this, R.color.chip_radius_background));
            chip.setTextColor(ContextCompat.getColorStateList(this, R.color.chip_radius_text));
            chip.setChipStrokeWidth(0f);
            chip.setElevation(4f);
            group.addView(chip);
            if (i == DEFAULT_RADIUS_INDEX) {
                chip.setChecked(true);
            }
        }

        group.setOnCheckedStateChangeListener((chipGroup, checkedIds) -> {
            if (checkedIds.isEmpty()) {
                return;
            }
            Chip checked = chipGroup.findViewById(checkedIds.get(0));
            radiusKm = (double) checked.getTag();
            if (!mapReady) {
                return;
            }
            if (radiusKm > 0 && userLocation == null && hasLocationPermission()) {
                locateUserThenLoad();
            } else {
                loadStations();
            }
        });
    }

    /**
     * Returns true if fine or coarse location permission has been granted.
     */
    private boolean hasLocationPermission() {
        return ContextCompat.checkSelfPermission(this, Manifest.permission.ACCESS_FINE_LOCATION)
                == PackageManager.PERMISSION_GRANTED
                || ContextCompat.checkSelfPermission(this, Manifest.permission.ACCESS_COARSE_LOCATION)
                == PackageManager.PERMISSION_GRANTED;
    }

    /**
     * Gets the phone's current location (falling back to the last known one),
     * then loads the stations near it.
     */
    @SuppressLint("MissingPermission") // only called after hasLocationPermission()
    private void locateUserThenLoad() {
        if (!hasLocationPermission()) {
            loadStations();
            return;
        }
        showLoading(true);
        locationClient.getCurrentLocation(Priority.PRIORITY_BALANCED_POWER_ACCURACY,
                        new CancellationTokenSource().getToken())
                .addOnSuccessListener(this, location -> {
                    if (location != null) {
                        userLocation = location;
                        loadStations();
                    } else {
                        useLastKnownLocation();
                    }
                })
                .addOnFailureListener(this, e -> useLastKnownLocation());
    }

    /**
     * Uses the last location saved by the phone when a fresh fix is not
     * available (for example when GPS is switched off).
     */
    @SuppressLint("MissingPermission") // only called after hasLocationPermission()
    private void useLastKnownLocation() {
        locationClient.getLastLocation()
                .addOnSuccessListener(this, location -> {
                    if (location != null) {
                        userLocation = location;
                    } else if (userLocation == null) {
                        Toast.makeText(this, R.string.map_location_unavailable, Toast.LENGTH_LONG).show();
                    }
                    loadStations();
                })
                .addOnFailureListener(this, e -> loadStations());
    }

    /**
     * Loads stations from the Web API: the nearby endpoint when the location
     * is known and a radius is selected, otherwise all active stations.
     * Falls back to the SQLite cache if the API cannot be reached.
     */
    private void loadStations() {
        if (!mapReady) {
            return;
        }
        showLoading(true);

        final boolean nearbySearch = userLocation != null && radiusKm > 0;
        Call<List<Station>> call = nearbySearch
                ? ApiClient.getService(this).getNearbyStations(
                userLocation.getLatitude(), userLocation.getLongitude(), radiusKm)
                : ApiClient.getService(this).getStations(true);

        call.enqueue(new Callback<List<Station>>() {
            @Override
            public void onResponse(@NonNull Call<List<Station>> call,
                                   @NonNull Response<List<Station>> response) {
                if (isFinishing() || isDestroyed()) {
                    return;
                }
                if (response.isSuccessful() && response.body() != null) {
                    // Save for offline use. A nearby result is only part of the list, so it is merged
                    if (nearbySearch) {
                        stationDao.upsertStations(response.body());
                    } else {
                        stationDao.cacheStations(response.body());
                    }
                    showStations(response.body(), false);
                } else {
                    showStations(filterCachedByRadius(), true);
                }
            }

            @Override
            public void onFailure(@NonNull Call<List<Station>> call, @NonNull Throwable t) {
                if (!isFinishing() && !isDestroyed()) {
                    showStations(filterCachedByRadius(), true);
                }
            }
        });
    }

    /**
     * Returns the cached stations inside the selected radius, used only when
     * the phone is offline and the API cannot do the nearby search.
     */
    private List<Station> filterCachedByRadius() {
        List<Station> cached = stationDao.getCachedStations();
        if (userLocation == null || radiusKm <= 0) {
            return cached;
        }
        List<Station> result = new ArrayList<>();
        for (Station s : cached) {
            if (distanceMetres(s) <= radiusKm * 1000) {
                result.add(s);
            }
        }
        return result;
    }

    /**
     * Draws a marker for every station, updates the subtitle and the
     * offline banner, and moves the camera so all stations are visible.
     *
     * @param list    stations to show
     * @param offline true when the list came from the SQLite cache
     */
    private void showStations(List<Station> list, boolean offline) {
        showLoading(false);
        tvOfflineBanner.setVisibility(offline ? View.VISIBLE : View.GONE);

        stations.clear();
        stations.addAll(list);
        if (userLocation != null) {
            Collections.sort(stations, Comparator.comparingDouble(this::distanceMetres));
        }
        stationMap.showStations(stations);

        // Keep the summary card open if the selected station is still in the list
        Station reselect = null;
        for (Station s : stations) {
            if (selectedStation != null && s.id != null && s.id.equals(selectedStation.id)) {
                reselect = s;
            }
        }

        updateSubtitle();
        if (reselect != null) {
            showStationCard(reselect);
        } else {
            hideStationCard();
        }
        fitCameraToStations();

        if (stations.isEmpty()) {
            Toast.makeText(this, R.string.map_no_stations, Toast.LENGTH_SHORT).show();
        }
    }

    /**
     * Writes a short summary under the title, e.g. "3 stations within 25 km · nearest 1.2 km".
     */
    private void updateSubtitle() {
        int count = stations.size();
        if (count == 0) {
            tvMapSubtitle.setText(R.string.map_no_stations);
            return;
        }
        String plural = count == 1 ? "station" : "stations";
        StringBuilder text = new StringBuilder();
        if (userLocation != null && radiusKm > 0) {
            text.append(String.format(Locale.ENGLISH, "%d %s within %.0f km", count, plural, radiusKm));
        } else {
            text.append(String.format(Locale.ENGLISH, "%d active %s", count, plural));
        }
        if (userLocation != null) {
            text.append(" · nearest ").append(formatDistance(distanceMetres(stations.get(0))));
        }
        tvMapSubtitle.setText(text.toString());
    }

    /**
     * Moves the camera so every station (and the user) fits on screen.
     */
    private void fitCameraToStations() {
        if (stations.isEmpty()) {
            if (userLocation != null) {
                stationMap.moveTo(userLocation.getLatitude(), userLocation.getLongitude(), 12, true);
            }
            return;
        }

        if (stations.size() == 1 && userLocation == null) {
            Station only = stations.get(0);
            stationMap.moveTo(only.latitude, only.longitude, 14, true);
            return;
        }

        List<double[]> points = new ArrayList<>();
        for (Station s : stations) {
            points.add(new double[]{s.latitude, s.longitude});
        }
        if (userLocation != null) {
            points.add(new double[]{userLocation.getLatitude(), userLocation.getLongitude()});
        }
        stationMap.fitTo(points, (int) (56 * getResources().getDisplayMetrics().density));
    }

    /**
     * Shows the station in the bottom card and centres the camera on it.
     */
    private void selectStation(Station station) {
        showStationCard(station);
        stationMap.moveTo(station.latitude, station.longitude, 14, true);
    }

    /**
     * Fills in and shows the bottom summary card for a station.
     */
    private void showStationCard(Station station) {
        selectedStation = station;
        tvCardName.setText(station.name);
        tvCardAddress.setText(station.address);
        tvCardCapacity.setText(String.format(Locale.ENGLISH, "%.0f kW/h", station.capacityKw));
        tvCardSlots.setText(String.format(Locale.ENGLISH, "%d slots", station.batterySlots));
        if (userLocation != null) {
            tvCardDistance.setText(formatDistance(distanceMetres(station)));
            tvCardDistance.setVisibility(View.VISIBLE);
        } else {
            tvCardDistance.setVisibility(View.GONE);
        }
        cardStation.setVisibility(View.VISIBLE);
    }

    /**
     * Hides the bottom summary card.
     */
    private void hideStationCard() {
        selectedStation = null;
        cardStation.setVisibility(View.GONE);
    }

    /**
     * Opens the station details screen for the given station.
     */
    private void openStationDetails(Station station) {
        if (station == null) {
            return;
        }
        Intent intent = new Intent(this, StationDetailsActivity.class);
        intent.putExtra(StationDetailsActivity.EXTRA_STATION, station);
        if (userLocation != null) {
            intent.putExtra(StationDetailsActivity.EXTRA_DISTANCE_METRES, distanceMetres(station));
        }
        startActivity(intent);
    }

    /**
     * Opens turn-by-turn directions to the station in the Google Maps app,
     * or in the browser if the app is not installed. Needs no API key.
     */
    static void openDirections(Context context, Station station) {
        if (station == null) {
            return;
        }
        String destination = station.latitude + "," + station.longitude;
        Intent navigation = new Intent(Intent.ACTION_VIEW, Uri.parse("google.navigation:q=" + destination));
        navigation.setPackage("com.google.android.apps.maps");
        try {
            context.startActivity(navigation);
        } catch (ActivityNotFoundException e) {
            context.startActivity(new Intent(Intent.ACTION_VIEW,
                    Uri.parse("https://www.google.com/maps/dir/?api=1&destination=" + destination)));
        }
    }

    /**
     * Centres the map on the user, asking for location permission first if needed.
     */
    private void onMyLocationClicked() {
        if (!hasLocationPermission()) {
            locationPermissionLauncher.launch(new String[]{
                    Manifest.permission.ACCESS_FINE_LOCATION,
                    Manifest.permission.ACCESS_COARSE_LOCATION});
            return;
        }
        if (userLocation != null && mapReady) {
            stationMap.moveTo(userLocation.getLatitude(), userLocation.getLongitude(), 13, true);
        }
        locateUserThenLoad();
    }

    /**
     * Reloads the location and the stations.
     */
    private void refresh() {
        if (!mapReady) {
            return;
        }
        if (hasLocationPermission()) {
            locateUserThenLoad();
        } else {
            loadStations();
        }
    }

    /**
     * Tells the map how much space the floating top bar and bottom card cover.
     */
    private void updateMapPadding() {
        if (!mapReady) {
            return;
        }
        int bottom = cardStation.getVisibility() == View.VISIBLE ? layoutBottomOverlay.getHeight() : 0;
        stationMap.setPadding(layoutTopOverlay.getHeight(), bottom);
    }

    /**
     * Shows or hides the thin loading bar under the title.
     */
    private void showLoading(boolean loading) {
        progressMap.setVisibility(loading ? View.VISIBLE : View.GONE);
        if (loading) {
            tvMapSubtitle.setText(R.string.map_loading);
        }
    }

    /**
     * Straight-line distance in metres from the user to a station
     * (used only for display and sorting).
     */
    private double distanceMetres(Station station) {
        if (userLocation == null) {
            return 0;
        }
        float[] result = new float[1];
        Location.distanceBetween(userLocation.getLatitude(), userLocation.getLongitude(),
                station.latitude, station.longitude, result);
        return result[0];
    }

    /**
     * Formats a distance as "850 m" or "2.4 km".
     */
    static String formatDistance(double metres) {
        if (metres < 1000) {
            return String.format(Locale.ENGLISH, "%.0f m", metres);
        }
        return String.format(Locale.ENGLISH, "%.1f km", metres / 1000);
    }
}
