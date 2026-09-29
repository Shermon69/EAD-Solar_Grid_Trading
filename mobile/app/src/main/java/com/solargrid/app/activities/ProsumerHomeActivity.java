/*
 * File:        ProsumerHomeActivity.java
 * Author:      Shermon H (IT22177964)
 * Description: Home screen for solar prosumers. Shows the user's name and a
 *              menu of prosumer features. Each member connects their own
 *              screen to the matching card below.
 * Created:     29/09/2026
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
 * Prosumer home screen with menu cards.
 */
public class ProsumerHomeActivity extends AppCompatActivity {

    private SessionManager session;

    /**
     * Sets up the header and the menu cards.
     */
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_prosumer_home);

        session = new SessionManager(this);

        TextView tvGreeting = findViewById(R.id.tvGreeting);
        TextView tvRole = findViewById(R.id.tvRole);
        tvGreeting.setText("Hello,");
        tvRole.setText(R.string.prosumer_role);
        findViewById(R.id.btnLogout).setOnClickListener(v -> confirmLogout());

        // Member 2: Dashboard -> startActivity(new Intent(this, DashboardActivity.class))
        MenuCardHelper.setup(findViewById(R.id.cardDashboard), R.drawable.ic_dashboard,
                R.string.menu_dashboard, R.string.menu_dashboard_desc, R.color.navy, v -> comingSoon());

        // Member 4: New Booking -> startActivity(new Intent(this, CreateBookingActivity.class))
        MenuCardHelper.setup(findViewById(R.id.cardNewBooking), R.drawable.ic_add,
                R.string.menu_new_booking, R.string.menu_new_booking_desc, R.color.solar, v -> comingSoon());

        // Member 2: My Bookings -> startActivity(new Intent(this, MyBookingsActivity.class))
        MenuCardHelper.setup(findViewById(R.id.cardMyBookings), R.drawable.ic_list,
                R.string.menu_my_bookings, R.string.menu_my_bookings_desc, R.color.purple, v -> comingSoon());

        // Member 3: Nearby Stations -> startActivity(new Intent(this, StationsMapActivity.class))
        MenuCardHelper.setup(findViewById(R.id.cardNearby), R.drawable.ic_place,
                R.string.menu_nearby, R.string.menu_nearby_desc, R.color.success, v -> comingSoon());

        // Member 2: Profile -> startActivity(new Intent(this, ProfileActivity.class))
        MenuCardHelper.setup(findViewById(R.id.cardProfile), R.drawable.ic_person,
                R.string.menu_profile, R.string.menu_profile_desc, R.color.info, v -> comingSoon());
    }

    /**
     * Refreshes the name every time the screen is shown (it may change after editing the profile).
     */
    @Override
    protected void onResume() {
        super.onResume();
        TextView tvUserName = findViewById(R.id.tvUserName);
        tvUserName.setText(session.getFullName());
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
