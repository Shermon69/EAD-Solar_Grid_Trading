/*
 * File:        StationDetailsActivity.java
 * Author:      WMVSB Wahundeniya (IT22292872)
 * Description: Displays details of a specific Solar Station when a map marker is tapped.
 */
package com.solargrid.app.activities;

import android.os.Bundle;
import android.widget.TextView;
import androidx.appcompat.app.AppCompatActivity;
import com.solargrid.app.R;
import com.solargrid.app.models.Station;

public class StationDetailsActivity extends AppCompatActivity {
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_station_details);
        setTitle("Station Details");

        Station station = (Station) getIntent().getSerializableExtra("station");
        if (station != null) {
            ((TextView) findViewById(R.id.tvStationName)).setText(station.name);
            ((TextView) findViewById(R.id.tvStationAddress)).setText(station.address);
            ((TextView) findViewById(R.id.tvStationCapacity)).setText("Capacity: " + station.capacityKw + " kW/h");
            ((TextView) findViewById(R.id.tvBatterySlots)).setText("Total Battery Slots: " + station.batterySlots);
            
            TextView tvStatus = findViewById(R.id.tvStatus);
            if (station.isActive) {
                tvStatus.setText("Status: ACTIVE");
                tvStatus.setTextColor(getResources().getColor(android.R.color.holo_green_dark, getTheme()));
            } else {
                tvStatus.setText("Status: DEACTIVATED");
                tvStatus.setTextColor(getResources().getColor(android.R.color.holo_red_dark, getTheme()));
            }
        }
    }
}
