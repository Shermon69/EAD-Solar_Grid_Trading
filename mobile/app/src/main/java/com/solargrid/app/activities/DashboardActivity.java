/*
 * File:        DashboardActivity.java
 * Author:      Premaratne R.A.N.C (IT22050908)
 * Description: Prosumer dashboard: pending bookings count, approved future
 *              bookings count and shortcuts to bookings and profile.
 */
package com.solargrid.app.activities;

import android.content.Intent;
import android.os.Bundle;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;

import com.solargrid.app.R;
import com.solargrid.app.api.ApiClient;
import com.solargrid.app.models.DashboardResponse;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

/**
 * Prosumer dashboard screen. Shows the pending bookings count and the
 * approved future bookings count, and provides shortcuts to the bookings
 * list (current and history) and the profile screen.
 */
public class DashboardActivity extends AppCompatActivity {

    private TextView tvPending, tvApproved;

    /**
     * Sets up the dashboard buttons and binds the count TextViews.
     */
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_dashboard);

        // Bind count TextViews.
        tvPending  = findViewById(R.id.tvPendingCount);
        tvApproved = findViewById(R.id.tvApprovedCount);

        // Button shortcuts.
        findViewById(R.id.btnCurrent)
                .setOnClickListener(v -> openBookings("current"));

        findViewById(R.id.btnHistory)
                .setOnClickListener(v -> openBookings("history"));

        findViewById(R.id.btnProfile)
                .setOnClickListener(v ->
                        startActivity(new Intent(this, ProfileActivity.class)));
    }

    /**
     * Reloads the dashboard counts every time the screen becomes visible.
     * Called after {@link #onCreate} and after returning from a child screen.
     */
    @Override
    protected void onResume() {
        super.onResume();

        // Fetch counts asynchronously from the Web API.
        ApiClient.getService(this).getDashboard()
                .enqueue(new Callback<DashboardResponse>() {

                    /** Updates the TextViews with the two counts from the API. */
                    @Override
                    public void onResponse(Call<DashboardResponse> call,
                                           Response<DashboardResponse> response) {
                        if (response.isSuccessful() && response.body() != null) {
                            tvPending.setText(String.valueOf(response.body().pendingCount));
                            tvApproved.setText(String.valueOf(response.body().approvedFutureCount));
                        }
                    }

                    /** Handles network errors. */
                    @Override
                    public void onFailure(Call<DashboardResponse> call, Throwable t) {
                        Toast.makeText(DashboardActivity.this,
                                "Cannot reach the server.",
                                Toast.LENGTH_LONG).show();
                    }
                });
    }

    /**
     * Opens the bookings list in "current" or "history" mode. The type is
     * passed to the next activity via an Intent extra.
     *
     * @param type "current" or "history"
     */
    private void openBookings(String type) {
        Intent i = new Intent(this, BookingsActivity.class);
        i.putExtra(BookingsActivity.EXTRA_TYPE, type);
        startActivity(i);
    }
}