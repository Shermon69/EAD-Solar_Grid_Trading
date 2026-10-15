/*
 * File:        OsmStationMap.java
 * Author:      WMVSB Wahundeniya (IT22292872)
 * Description: StationMap implemented with osmdroid and free OpenStreetMap tiles.
 *              Used when the Google Maps key cannot be used (no key or no billing).
 *              Library: osmdroid (Apache 2.0, https://github.com/osmdroid/osmdroid).
 *              Map data © OpenStreetMap contributors (https://www.openstreetmap.org/copyright).
 * Created:     15/10/2026
 */

package com.solargrid.app.maps;

import android.graphics.drawable.Drawable;
import android.view.ViewGroup;

import androidx.appcompat.app.AppCompatActivity;
import androidx.core.content.ContextCompat;
import androidx.fragment.app.Fragment;
import androidx.fragment.app.FragmentManager;

import com.solargrid.app.R;
import com.solargrid.app.models.Station;

import org.osmdroid.events.MapEventsReceiver;
import org.osmdroid.tileprovider.tilesource.TileSourceFactory;
import org.osmdroid.util.BoundingBox;
import org.osmdroid.util.GeoPoint;
import org.osmdroid.views.CustomZoomButtonsController;
import org.osmdroid.views.MapView;
import org.osmdroid.views.overlay.CopyrightOverlay;
import org.osmdroid.views.overlay.MapEventsOverlay;
import org.osmdroid.views.overlay.Marker;
import org.osmdroid.views.overlay.mylocation.GpsMyLocationProvider;
import org.osmdroid.views.overlay.mylocation.MyLocationNewOverlay;

import java.util.ArrayList;
import java.util.List;

/**
 * Station map drawn with OpenStreetMap (no API key or billing needed).
 */
public class OsmStationMap implements StationMap {

    private AppCompatActivity activity;
    private MapView mapView;
    private MyLocationNewOverlay myLocationOverlay;
    private CopyrightOverlay copyrightOverlay;
    private final List<Marker> stationMarkers = new ArrayList<>();
    private Listener listener;
    private int topPadding;
    private int bottomPadding;

    /**
     * Creates the OpenStreetMap view inside the container.
     */
    @Override
    public void attach(AppCompatActivity activity, ViewGroup container, Runnable onReady) {
        this.activity = activity;

        // Remove a Google map left in the container after switching provider
        FragmentManager fm = activity.getSupportFragmentManager();
        Fragment existing = fm.findFragmentById(container.getId());
        if (existing != null) {
            fm.beginTransaction().remove(existing).commitNow();
        }

        StationMaps.configureOsm(activity);
        mapView = new MapView(activity);
        mapView.setTileSource(TileSourceFactory.MAPNIK);
        mapView.setMultiTouchControls(true);
        mapView.setTilesScaledToDpi(true);
        mapView.getZoomController().setVisibility(CustomZoomButtonsController.Visibility.NEVER);
        mapView.setMinZoomLevel(4.0);
        mapView.setMaxZoomLevel(19.0);

        // OpenStreetMap's licence requires the "© OpenStreetMap contributors" credit on the map
        copyrightOverlay = new CopyrightOverlay(activity);
        mapView.getOverlays().add(copyrightOverlay);

        // Taps on empty map areas (index 0 so markers get their taps first)
        mapView.getOverlays().add(0, new MapEventsOverlay(new MapEventsReceiver() {
            @Override
            public boolean singleTapConfirmedHelper(GeoPoint p) {
                if (listener != null) {
                    listener.onMapTapped();
                }
                return false;
            }

            @Override
            public boolean longPressHelper(GeoPoint p) {
                return false;
            }
        }));

        container.addView(mapView, new ViewGroup.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.MATCH_PARENT));

        // Camera moves need the map size, so report "ready" after the first layout
        mapView.addOnFirstLayoutListener((v, left, top, right, bottom) -> onReady.run());
    }

    /**
     * Sets who receives marker and map tap events.
     */
    @Override
    public void setListener(Listener listener) {
        this.listener = listener;
    }

    /**
     * Adds or removes the "you are here" overlay that follows the phone's GPS.
     */
    @Override
    public void setMyLocationEnabled(boolean enabled) {
        if (mapView == null) {
            return;
        }
        if (enabled && myLocationOverlay == null) {
            myLocationOverlay = new MyLocationNewOverlay(new GpsMyLocationProvider(activity), mapView);
            myLocationOverlay.enableMyLocation();
            mapView.getOverlays().add(myLocationOverlay);
        } else if (!enabled && myLocationOverlay != null) {
            myLocationOverlay.disableMyLocation();
            mapView.getOverlays().remove(myLocationOverlay);
            myLocationOverlay = null;
        }
        mapView.invalidate();
    }

    /**
     * Replaces the station markers. Tapping one tells the listener instead of
     * opening osmdroid's own bubble, because the screen shows its own card.
     */
    @Override
    public void showStations(List<Station> stations) {
        if (mapView == null) {
            return;
        }
        mapView.getOverlays().removeAll(stationMarkers);
        stationMarkers.clear();

        Drawable activeIcon = ContextCompat.getDrawable(activity, R.drawable.ic_map_pin);
        Drawable inactiveIcon = ContextCompat.getDrawable(activity, R.drawable.ic_map_pin_inactive);

        for (Station s : stations) {
            Marker marker = new Marker(mapView);
            marker.setPosition(new GeoPoint(s.latitude, s.longitude));
            marker.setTitle(s.name);
            marker.setSnippet(s.address);
            marker.setIcon(s.isActive ? activeIcon : inactiveIcon);
            marker.setAnchor(Marker.ANCHOR_CENTER, Marker.ANCHOR_BOTTOM);
            marker.setInfoWindow(null);
            marker.setOnMarkerClickListener((m, map) -> {
                if (listener != null) {
                    listener.onStationTapped(s);
                }
                return true;
            });
            stationMarkers.add(marker);
        }
        mapView.getOverlays().addAll(stationMarkers);
        mapView.invalidate();
    }

    /**
     * Moves the camera to a point.
     */
    @Override
    public void moveTo(double latitude, double longitude, float zoom, boolean animate) {
        if (mapView == null) {
            return;
        }
        GeoPoint point = new GeoPoint(latitude, longitude);
        if (animate) {
            mapView.getController().animateTo(point, (double) zoom, 600L);
        } else {
            mapView.getController().setZoom((double) zoom);
            mapView.getController().setCenter(point);
        }
    }

    /**
     * Zooms so all points fit. osmdroid has no map padding, so the border is
     * made larger by the height of the floating overlays.
     */
    @Override
    public void fitTo(List<double[]> points, int paddingPx) {
        if (mapView == null || points.isEmpty()) {
            return;
        }
        if (points.size() == 1) {
            moveTo(points.get(0)[0], points.get(0)[1], 14, true);
            return;
        }
        List<GeoPoint> geoPoints = new ArrayList<>();
        for (double[] p : points) {
            geoPoints.add(new GeoPoint(p[0], p[1]));
        }
        BoundingBox box = BoundingBox.fromGeoPoints(geoPoints);
        int border = paddingPx + Math.max(topPadding, bottomPadding);
        mapView.zoomToBoundingBox(box, true, border);
    }

    /**
     * Remembers the overlay sizes and lifts the map credit above the bottom card.
     */
    @Override
    public void setPadding(int topPx, int bottomPx) {
        topPadding = topPx;
        bottomPadding = bottomPx;
        if (copyrightOverlay != null && mapView != null) {
            int margin = (int) (8 * activity.getResources().getDisplayMetrics().density);
            copyrightOverlay.setAlignBottom(true);
            copyrightOverlay.setOffset(margin, bottomPx + margin);
            mapView.invalidate();
        }
    }

    /**
     * Restarts tile loading and GPS tracking when the screen is visible again.
     */
    @Override
    public void onResume() {
        if (mapView != null) {
            mapView.onResume();
        }
        if (myLocationOverlay != null) {
            myLocationOverlay.enableMyLocation();
        }
    }

    /**
     * Stops tile loading and GPS tracking to save battery.
     */
    @Override
    public void onPause() {
        if (myLocationOverlay != null) {
            myLocationOverlay.disableMyLocation();
        }
        if (mapView != null) {
            mapView.onPause();
        }
    }

    /**
     * Releases the map's resources.
     */
    @Override
    public void onDestroy() {
        if (mapView != null) {
            mapView.onDetach();
        }
    }
}
