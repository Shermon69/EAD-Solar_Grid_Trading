/*
 * File:        StationMaps.java
 * Author:      WMVSB Wahundeniya (IT22292872)
 * Description: Chooses which map to use (Google Maps or the free OpenStreetMap
 *              fallback), remembers the user's choice and draws the small
 *              station preview map used on the station details screen.
 * Created:     15/10/2026
 */

package com.solargrid.app.maps;

import android.annotation.SuppressLint;
import android.content.Context;
import android.content.SharedPreferences;
import android.content.pm.ApplicationInfo;
import android.content.pm.PackageManager;
import android.view.MotionEvent;
import android.view.ViewGroup;

import androidx.appcompat.app.AppCompatActivity;
import androidx.core.content.ContextCompat;
import androidx.fragment.app.Fragment;
import androidx.fragment.app.FragmentManager;

import com.google.android.gms.maps.CameraUpdateFactory;
import com.google.android.gms.maps.GoogleMapOptions;
import com.google.android.gms.maps.SupportMapFragment;
import com.google.android.gms.maps.model.BitmapDescriptorFactory;
import com.google.android.gms.maps.model.LatLng;
import com.google.android.gms.maps.model.MarkerOptions;
import com.solargrid.app.BuildConfig;
import com.solargrid.app.R;
import com.solargrid.app.models.Station;

import org.osmdroid.config.Configuration;
import org.osmdroid.config.IConfigurationProvider;
import org.osmdroid.tileprovider.tilesource.TileSourceFactory;
import org.osmdroid.util.GeoPoint;
import org.osmdroid.views.CustomZoomButtonsController;
import org.osmdroid.views.MapView;
import org.osmdroid.views.overlay.CopyrightOverlay;
import org.osmdroid.views.overlay.Marker;

import java.io.File;

/**
 * Helper methods for picking and creating the station map.
 */
public final class StationMaps {

    public static final String PROVIDER_GOOGLE = "google";
    public static final String PROVIDER_OSM = "osm";

    private static final String PREFS = "map_settings";
    private static final String KEY_PROVIDER = "provider";

    /**
     * Private constructor: this class only has static methods.
     */
    private StationMaps() {
    }

    /**
     * Returns the map to use: the user's last choice, otherwise MAP_PROVIDER from
     * local.properties. Google is only used when a Maps API key is set.
     */
    public static String getProvider(Context context) {
        String saved = context.getSharedPreferences(PREFS, Context.MODE_PRIVATE)
                .getString(KEY_PROVIDER, null);
        String provider = saved != null ? saved : BuildConfig.MAP_PROVIDER;
        if (PROVIDER_GOOGLE.equalsIgnoreCase(provider) && hasGoogleKey(context)) {
            return PROVIDER_GOOGLE;
        }
        return PROVIDER_OSM;
    }

    /**
     * Saves the map chosen with the layer button on the map screen.
     */
    public static void setProvider(Context context, String provider) {
        SharedPreferences prefs = context.getSharedPreferences(PREFS, Context.MODE_PRIVATE);
        prefs.edit().putString(KEY_PROVIDER, provider).apply();
    }

    /**
     * Returns true if a Google Maps API key was added to the manifest.
     */
    public static boolean hasGoogleKey(Context context) {
        try {
            ApplicationInfo info = context.getPackageManager()
                    .getApplicationInfo(context.getPackageName(), PackageManager.GET_META_DATA);
            String key = info.metaData == null ? null
                    : info.metaData.getString("com.google.android.geo.API_KEY");
            return key != null && !key.trim().isEmpty();
        } catch (PackageManager.NameNotFoundException e) {
            return false;
        }
    }

    /**
     * Creates the map for the current provider.
     */
    public static StationMap create(Context context) {
        return PROVIDER_GOOGLE.equals(getProvider(context)) ? new GoogleStationMap() : new OsmStationMap();
    }

    /**
     * osmdroid setup: identifies the app to the OpenStreetMap tile servers (their
     * usage policy requires this) and keeps downloaded tiles in the app's cache.
     */
    static void configureOsm(Context context) {
        IConfigurationProvider config = Configuration.getInstance();
        config.load(context, context.getSharedPreferences("osmdroid", Context.MODE_PRIVATE));
        config.setUserAgentValue(BuildConfig.APPLICATION_ID);
        File base = new File(context.getCacheDir(), "osmdroid");
        config.setOsmdroidBasePath(base);
        config.setOsmdroidTileCache(new File(base, "tiles"));
    }

    /**
     * Draws a small, non-interactive map of one station into the container.
     * Tapping it runs onClick (used to open directions).
     */
    @SuppressLint("ClickableViewAccessibility") // the whole preview acts as one button
    public static void showPreview(AppCompatActivity activity, ViewGroup container,
                                   Station station, Runnable onClick) {
        FragmentManager fm = activity.getSupportFragmentManager();
        Fragment existing = fm.findFragmentById(container.getId());

        if (PROVIDER_GOOGLE.equals(getProvider(activity))) {
            SupportMapFragment fragment;
            if (existing instanceof SupportMapFragment) {
                fragment = (SupportMapFragment) existing;
            } else {
                // Lite mode: a light, static image of the map
                fragment = SupportMapFragment.newInstance(
                        new GoogleMapOptions().liteMode(true).mapToolbarEnabled(false));
                fm.beginTransaction().replace(container.getId(), fragment).commitNow();
            }
            fragment.getMapAsync(map -> {
                LatLng position = new LatLng(station.latitude, station.longitude);
                map.clear();
                map.addMarker(new MarkerOptions()
                        .position(position)
                        .title(station.name)
                        .icon(BitmapDescriptorFactory.defaultMarker(BitmapDescriptorFactory.HUE_ORANGE)));
                map.moveCamera(CameraUpdateFactory.newLatLngZoom(position, 15));
                map.setOnMapClickListener(latLng -> onClick.run());
            });
            return;
        }

        if (existing != null) {
            fm.beginTransaction().remove(existing).commitNow();
        }
        container.removeAllViews();

        configureOsm(activity);
        MapView mapView = new MapView(activity);
        mapView.setTileSource(TileSourceFactory.MAPNIK);
        mapView.setTilesScaledToDpi(true);
        mapView.setMultiTouchControls(false);
        mapView.getZoomController().setVisibility(CustomZoomButtonsController.Visibility.NEVER);
        mapView.getController().setZoom(15.0);
        GeoPoint position = new GeoPoint(station.latitude, station.longitude);
        mapView.getController().setCenter(position);

        Marker marker = new Marker(mapView);
        marker.setPosition(position);
        marker.setIcon(ContextCompat.getDrawable(activity,
                station.isActive ? R.drawable.ic_map_pin : R.drawable.ic_map_pin_inactive));
        marker.setAnchor(Marker.ANCHOR_CENTER, Marker.ANCHOR_BOTTOM);
        marker.setInfoWindow(null);
        mapView.getOverlays().add(marker);
        mapView.getOverlays().add(new CopyrightOverlay(activity));

        // Block panning; a tap anywhere opens directions
        mapView.setOnTouchListener((v, event) -> {
            if (event.getAction() == MotionEvent.ACTION_UP) {
                onClick.run();
            }
            return true;
        });

        container.addView(mapView, new ViewGroup.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.MATCH_PARENT));
    }
}
