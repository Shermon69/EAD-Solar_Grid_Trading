/*
 * File:        MapActivity.java
 * Author:      WMVSB Wahundeniya (IT22292872)
 * Description: Displays nearby solar stations on a Google Map using markers.
 *              Tapping a marker can navigate to the StationDetailsActivity.
 * Created:     29/09/2026
 */

package com.solargrid.app.activities;

import android.Manifest;
import android.content.pm.PackageManager;
import android.os.Bundle;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.app.ActivityCompat;

import com.google.android.gms.maps.CameraUpdateFactory;
import com.google.android.gms.maps.GoogleMap;
import com.google.android.gms.maps.OnMapReadyCallback;
import com.google.android.gms.maps.SupportMapFragment;
import com.google.android.gms.maps.model.LatLng;
import com.google.android.gms.maps.model.MarkerOptions;
import com.solargrid.app.R;
import com.solargrid.app.db.DatabaseHelper;
import com.solargrid.app.db.StationDao;
import com.solargrid.app.models.Station;

import java.util.List;

public class MapActivity extends AppCompatActivity implements OnMapReadyCallback {

    private GoogleMap mMap;
    private StationDao stationDao;
    private static final int LOCATION_PERMISSION_REQUEST_CODE = 1;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_map);

        stationDao = new StationDao(DatabaseHelper.getInstance(this));

        SupportMapFragment mapFragment = (SupportMapFragment) getSupportFragmentManager()
                .findFragmentById(R.id.map);
        if (mapFragment != null) {
            mapFragment.getMapAsync(this);
        }
    }

    @Override
    public void onMapReady(@NonNull GoogleMap googleMap) {
        mMap = googleMap;

        enableMyLocation();
        loadStationsOnMap();
    }

    /**
     * Executes the enableMyLocation operation.
     */
    private void enableMyLocation() {
        if (ActivityCompat.checkSelfPermission(this, Manifest.permission.ACCESS_FINE_LOCATION) 
                == PackageManager.PERMISSION_GRANTED || 
            ActivityCompat.checkSelfPermission(this, Manifest.permission.ACCESS_COARSE_LOCATION) 
                == PackageManager.PERMISSION_GRANTED) {
            mMap.setMyLocationEnabled(true);
        } else {
            ActivityCompat.requestPermissions(this,
                    new String[]{Manifest.permission.ACCESS_FINE_LOCATION},
                    LOCATION_PERMISSION_REQUEST_CODE);
        }
    }

    /**
     * Executes the loadStationsOnMap operation.
     */
    private void loadStationsOnMap() {
        com.solargrid.app.api.ApiService apiService = com.solargrid.app.api.ApiClient.getClient().create(com.solargrid.app.api.ApiService.class);
        apiService.getStations(true).enqueue(new retrofit2.Callback<List<Station>>() {
            @Override
            public void onResponse(@NonNull retrofit2.Call<List<Station>> call, @NonNull retrofit2.Response<List<Station>> response) {
                if (response.isSuccessful() && response.body() != null) {
                    stationDao.cacheStations(response.body());
                    displayStationsOnMap(response.body());
                } else {
                    displayStationsOnMap(stationDao.getCachedStations());
                }
            }

            @Override
            public void onFailure(@NonNull retrofit2.Call<List<Station>> call, @NonNull Throwable t) {
                displayStationsOnMap(stationDao.getCachedStations());
                Toast.makeText(MapActivity.this, "Failed to load from API. Showing cached stations.", Toast.LENGTH_SHORT).show();
            }
        });
    }

    /**
     * Executes the displayStationsOnMap operation.
     */
    private void displayStationsOnMap(List<Station> stations) {
        if (stations.isEmpty()) {
            Toast.makeText(this, "No stations cached locally.", Toast.LENGTH_SHORT).show();
            return;
        }

        mMap.clear();
        for (Station s : stations) {
            LatLng location = new LatLng(s.latitude, s.longitude);
            mMap.addMarker(new MarkerOptions()
                    .position(location)
                    .title(s.name)
                    .snippet(s.address + " | Capacity: " + s.capacityKw + "kW"));
        }

        // Move camera to the first station
        Station first = stations.get(0);
        mMap.moveCamera(CameraUpdateFactory.newLatLngZoom(new LatLng(first.latitude, first.longitude), 12));

        mMap.setOnInfoWindowClickListener(marker -> {
            for (Station s : stations) {
                if (s.name.equals(marker.getTitle())) {
                    android.content.Intent intent = new android.content.Intent(MapActivity.this, StationDetailsActivity.class);
                    intent.putExtra("station", s);
                    startActivity(intent);
                    break;
                }
            }
        });
    }

    @Override
    public void onRequestPermissionsResult(int requestCode, @NonNull String[] permissions, @NonNull int[] grantResults) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults);
        if (requestCode == LOCATION_PERMISSION_REQUEST_CODE) {
            if (grantResults.length > 0 && grantResults[0] == PackageManager.PERMISSION_GRANTED) {
                enableMyLocation();
            }
        }
    }
}
