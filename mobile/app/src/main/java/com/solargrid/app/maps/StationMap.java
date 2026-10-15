/*
 * File:        StationMap.java
 * Author:      WMVSB Wahundeniya (IT22292872)
 * Description: Common interface for the station map, so the map screen works the
 *              same with Google Maps (GoogleStationMap) or the free OpenStreetMap
 *              fallback (OsmStationMap).
 * Created:     15/10/2026
 */

package com.solargrid.app.maps;

import android.view.ViewGroup;

import androidx.appcompat.app.AppCompatActivity;

import com.solargrid.app.models.Station;

import java.util.List;

/**
 * A map that can show solar stations as markers.
 */
public interface StationMap {

    /**
     * Events sent from the map back to the screen.
     */
    interface Listener {

        /**
         * A station marker was tapped.
         */
        void onStationTapped(Station station);

        /**
         * The station's info bubble was tapped (open its details).
         */
        void onStationDetailsTapped(Station station);

        /**
         * An empty part of the map was tapped.
         */
        void onMapTapped();
    }

    /**
     * Adds the map into the container and calls onReady once it can be used.
     */
    void attach(AppCompatActivity activity, ViewGroup container, Runnable onReady);

    /**
     * Sets who receives marker and map tap events.
     */
    void setListener(Listener listener);

    /**
     * Shows or hides the "you are here" dot. Location permission must already be granted.
     */
    void setMyLocationEnabled(boolean enabled);

    /**
     * Removes the old markers and adds one marker per station.
     */
    void showStations(List<Station> stations);

    /**
     * Moves the camera to a point at the given zoom level.
     */
    void moveTo(double latitude, double longitude, float zoom, boolean animate);

    /**
     * Moves the camera so every point ({latitude, longitude}) is visible.
     */
    void fitTo(List<double[]> points, int paddingPx);

    /**
     * Space covered by the floating top bar and bottom card, so map controls stay visible.
     */
    void setPadding(int topPx, int bottomPx);

    /**
     * Forwarded from the activity's onResume.
     */
    void onResume();

    /**
     * Forwarded from the activity's onPause.
     */
    void onPause();

    /**
     * Forwarded from the activity's onDestroy.
     */
    void onDestroy();
}
