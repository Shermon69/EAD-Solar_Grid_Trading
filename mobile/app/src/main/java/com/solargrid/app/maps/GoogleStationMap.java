/*
 * File:        GoogleStationMap.java
 * Author:      WMVSB Wahundeniya (IT22292872)
 * Description: StationMap implemented with the Google Maps SDK for Android.
 *              Needs MAPS_API_KEY in local.properties from a Google Cloud
 *              project with billing and "Maps SDK for Android" enabled.
 * Created:     15/10/2026
 */

package com.solargrid.app.maps;

import android.annotation.SuppressLint;
import android.view.ViewGroup;

import androidx.appcompat.app.AppCompatActivity;
import androidx.fragment.app.Fragment;
import androidx.fragment.app.FragmentManager;

import com.google.android.gms.maps.CameraUpdate;
import com.google.android.gms.maps.CameraUpdateFactory;
import com.google.android.gms.maps.GoogleMap;
import com.google.android.gms.maps.SupportMapFragment;
import com.google.android.gms.maps.model.BitmapDescriptorFactory;
import com.google.android.gms.maps.model.LatLng;
import com.google.android.gms.maps.model.LatLngBounds;
import com.google.android.gms.maps.model.Marker;
import com.google.android.gms.maps.model.MarkerOptions;
import com.solargrid.app.models.Station;

import java.util.List;

/**
 * Station map drawn by Google Maps.
 */
public class GoogleStationMap implements StationMap {

    private GoogleMap map;
    private Listener listener;

    /**
     * Adds a SupportMapFragment to the container and waits for the map to load.
     */
    @Override
    public void attach(AppCompatActivity activity, ViewGroup container, Runnable onReady) {
        FragmentManager fm = activity.getSupportFragmentManager();
        Fragment existing = fm.findFragmentById(container.getId());
        SupportMapFragment fragment;
        if (existing instanceof SupportMapFragment) {
            fragment = (SupportMapFragment) existing;
        } else {
            fragment = SupportMapFragment.newInstance();
            fm.beginTransaction().replace(container.getId(), fragment).commitNow();
        }

        fragment.getMapAsync(googleMap -> {
            map = googleMap;
            map.getUiSettings().setMyLocationButtonEnabled(false);
            map.getUiSettings().setMapToolbarEnabled(false);
            map.getUiSettings().setCompassEnabled(true);

            map.setOnMarkerClickListener(marker -> {
                if (listener != null && marker.getTag() instanceof Station) {
                    listener.onStationTapped((Station) marker.getTag());
                }
                return false; // also centre on the marker and open its info window
            });
            map.setOnInfoWindowClickListener(marker -> {
                if (listener != null && marker.getTag() instanceof Station) {
                    listener.onStationDetailsTapped((Station) marker.getTag());
                }
            });
            map.setOnMapClickListener(latLng -> {
                if (listener != null) {
                    listener.onMapTapped();
                }
            });
            onReady.run();
        });
    }

    /**
     * Sets who receives marker and map tap events.
     */
    @Override
    public void setListener(Listener listener) {
        this.listener = listener;
    }

    /**
     * Turns Google's blue location dot on or off.
     */
    @SuppressLint("MissingPermission") // the screen checks location permission first
    @Override
    public void setMyLocationEnabled(boolean enabled) {
        if (map != null) {
            map.setMyLocationEnabled(enabled);
        }
    }

    /**
     * Draws an orange marker per active station (red if deactivated).
     */
    @Override
    public void showStations(List<Station> stations) {
        if (map == null) {
            return;
        }
        map.clear();
        for (Station s : stations) {
            Marker marker = map.addMarker(new MarkerOptions()
                    .position(new LatLng(s.latitude, s.longitude))
                    .title(s.name)
                    .snippet(s.address)
                    .icon(BitmapDescriptorFactory.defaultMarker(
                            s.isActive ? BitmapDescriptorFactory.HUE_ORANGE : BitmapDescriptorFactory.HUE_RED)));
            if (marker != null) {
                marker.setTag(s);
            }
        }
    }

    /**
     * Moves the camera to a point.
     */
    @Override
    public void moveTo(double latitude, double longitude, float zoom, boolean animate) {
        if (map == null) {
            return;
        }
        CameraUpdate update = CameraUpdateFactory.newLatLngZoom(new LatLng(latitude, longitude), zoom);
        if (animate) {
            map.animateCamera(update);
        } else {
            map.moveCamera(update);
        }
    }

    /**
     * Moves the camera so all points fit on screen.
     */
    @Override
    public void fitTo(List<double[]> points, int paddingPx) {
        if (map == null || points.isEmpty()) {
            return;
        }
        LatLngBounds.Builder bounds = new LatLngBounds.Builder();
        for (double[] p : points) {
            bounds.include(new LatLng(p[0], p[1]));
        }
        try {
            map.animateCamera(CameraUpdateFactory.newLatLngBounds(bounds.build(), paddingPx));
        } catch (IllegalStateException e) {
            // Map has not been measured yet; jump straight to the first point instead
            moveTo(points.get(0)[0], points.get(0)[1], 12, false);
        }
    }

    /**
     * Keeps the Google logo and camera moves clear of the floating overlays.
     */
    @Override
    public void setPadding(int topPx, int bottomPx) {
        if (map != null) {
            map.setPadding(0, topPx, 0, bottomPx);
        }
    }

    /**
     * Nothing to do: the map fragment follows the activity lifecycle itself.
     */
    @Override
    public void onResume() {
    }

    /**
     * Nothing to do: the map fragment follows the activity lifecycle itself.
     */
    @Override
    public void onPause() {
    }

    /**
     * Nothing to do: the map fragment follows the activity lifecycle itself.
     */
    @Override
    public void onDestroy() {
    }
}
