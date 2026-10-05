/*
 * File:        OperatorHomeActivity.java
 * Author:      Shermon H (IT22177964)
 * Description: Home screen for grid operators (Operator mode). From here the
 *              operator scans prosumer QR codes and views solar stations.
 */

package com.solargrid.app.activities;

import android.content.Intent;
import android.os.Bundle;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;

import com.solargrid.app.R;
import com.solargrid.app.db.SessionManager;
import com.solargrid.app.utils.MenuCardHelper;

/**
 * Grid Operator home screen.
 */
public class OperatorHomeActivity extends AppCompatActivity {

    private SessionManager session;

    /**
     * Sets up the header and the operator menu cards.
     */
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_operator_home);

        session = new SessionManager(this);

        TextView tvGreeting = findViewById(R.id.tvGreeting);
        TextView tvUserName = findViewById(R.id.tvUserName);
        TextView tvRole = findViewById(R.id.tvRole);
        tvGreeting.setText("Operator mode");
        tvUserName.setText(session.getFullName());
        tvRole.setText(R.string.operator_role);
        findViewById(R.id.btnLogout).setOnClickListener(v -> confirmLogout());

        // Member 3: Scan QR -> startActivity(new Intent(this, ScanQrActivity.class))
        MenuCardHelper.setup(findViewById(R.id.cardScanQr), R.drawable.ic_qr_scan,
                R.string.menu_scan_qr, R.string.menu_scan_qr_desc, R.color.solar, 
                v -> startActivity(new Intent(this, QrScanActivity.class)));

        // Member 3: Stations -> startActivity(new Intent(this, StationsMapActivity.class))
        MenuCardHelper.setup(findViewById(R.id.cardStations), R.drawable.ic_place,
                R.string.menu_stations, R.string.menu_stations_desc, R.color.success, 
                v -> startActivity(new Intent(this, MapActivity.class)));

        // Member 1: Bookings (approve or cancel prosumer bookings)
        MenuCardHelper.setup(findViewById(R.id.cardBookings), R.drawable.ic_list,
                R.string.menu_bookings, R.string.menu_bookings_desc, R.color.purple,
                v -> startActivity(new Intent(this, OperatorBookingsActivity.class)));
    }

    /**
     * Temporary action for features that are not built yet.
     */
    private void comingSoon() {
        Toast.makeText(this, R.string.coming_soon, Toast.LENGTH_SHORT).show();
    }

    /**
     * Asks for confirmation, then clears the session and returns to the login screen.
     */
    private void confirmLogout() {
        new AlertDialog.Builder(this)
                .setTitle(R.string.logout)
                .setMessage("Do you want to log out?")
                .setPositiveButton(R.string.logout, (dialog, which) -> {
                    session.logout();
                    Intent intent = new Intent(this, LoginActivity.class);
                    intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
                    startActivity(intent);
                })
                .setNegativeButton(android.R.string.cancel, null)
                .show();
    }
}
