/*
 * File:        MyReservationsActivity.java
 * Author:      Dissanayake D.M.S.N (IT22210692)
 * Description: Displays the prosumer's energy reservations retrieved
 *              from the central Web API.
 * Created:     30/09/2026
 */

package com.solargrid.app.activities;

import android.content.Intent;
import android.graphics.Color;
import android.os.Bundle;
import android.view.Gravity;
import android.view.View;
import android.widget.Button;
import android.widget.LinearLayout;
import android.widget.ProgressBar;
import android.widget.TextView;

import androidx.appcompat.app.AppCompatActivity;

import com.solargrid.app.R;
import com.solargrid.app.api.ApiClient;
import com.solargrid.app.api.ApiService;
import com.solargrid.app.models.Reservation;
import com.solargrid.app.utils.DateUtils;

import java.util.List;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

/**
 * Activity used to display the prosumer's reservations.
 */
public class MyReservationsActivity extends AppCompatActivity {

    private LinearLayout reservationsContainer;
    private ProgressBar progressReservations;
    private TextView tvNoReservations;

    private Button btnCurrentReservations;
    private Button btnHistoryReservations;

    private ApiService apiService;

    private boolean showingCurrent = true;

    /**
     * Creates the reservations screen.
     *
     * @param savedInstanceState previously saved activity state
     */
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        setContentView(R.layout.activity_my_reservations);

        initializeViews();

        apiService = ApiClient.getService(this);

        setupFilterButtons();

        loadReservations();
    }

    /**
     * Refreshes the reservation list whenever the activity
     * becomes visible again.
     *
     * This ensures that changes made in the Edit or Details
     * screen are immediately reflected in the list.
     */
    @Override
    protected void onResume() {
        super.onResume();

        /*
         * Avoid loading before the API service and views have
         * been initialized during the first activity creation.
         */
        if (apiService != null) {
            loadReservations();
        }
    }

    /**
     * Finds and stores references to the screen controls.
     */
    private void initializeViews() {

        reservationsContainer =
                findViewById(R.id.reservationsContainer);

        progressReservations =
                findViewById(R.id.progressReservations);

        tvNoReservations =
                findViewById(R.id.tvNoReservations);

        btnCurrentReservations =
                findViewById(R.id.btnCurrentReservations);

        btnHistoryReservations =
                findViewById(R.id.btnHistoryReservations);
    }

    /**
     * Configures the current and history filter buttons.
     */
    private void setupFilterButtons() {

        btnCurrentReservations.setOnClickListener(v -> {

            showingCurrent = true;

            updateFilterButtons();

            loadReservations();
        });

        btnHistoryReservations.setOnClickListener(v -> {

            showingCurrent = false;

            updateFilterButtons();

            loadReservations();
        });

        updateFilterButtons();
    }

    /**
     * Updates the enabled state of the filter buttons.
     */
    private void updateFilterButtons() {

        btnCurrentReservations.setEnabled(!showingCurrent);

        btnHistoryReservations.setEnabled(showingCurrent);
    }

    /**
     * Loads the prosumer's reservations from the central Web API.
     */
    private void loadReservations() {

        showLoading();

        String status = showingCurrent
                ? "current"
                : "history";

        apiService.getMyReservations(
                status,
                ""
        ).enqueue(new Callback<List<Reservation>>() {

            @Override
            public void onResponse(
                    Call<List<Reservation>> call,
                    Response<List<Reservation>> response
            ) {

                hideLoading();

                /*
                 * Show the actual HTTP status and API error message
                 * so that communication problems can be diagnosed.
                 */
                if (!response.isSuccessful()) {

                    String errorMessage =
                            "HTTP " + response.code();

                    if (response.errorBody() != null) {

                        try {

                            String serverMessage =
                                    response.errorBody().string();

                            if (serverMessage != null
                                    && !serverMessage.trim().isEmpty()) {

                                errorMessage +=
                                        "\n" + serverMessage;
                            }

                        } catch (Exception ignored) {
                            // Keep the HTTP status when the error body
                            // cannot be read.
                        }
                    }

                    showNoReservations(errorMessage);

                    return;
                }

                if (response.body() == null) {

                    showNoReservations(
                            "Server returned an empty response."
                    );

                    return;
                }

                displayReservations(response.body());
            }

            @Override
            public void onFailure(
                    Call<List<Reservation>> call,
                    Throwable t
            ) {

                hideLoading();

                String errorMessage =
                        "Unable to connect to server.";

                if (t.getMessage() != null
                        && !t.getMessage().trim().isEmpty()) {

                    errorMessage +=
                            "\n" + t.getMessage();
                }

                showNoReservations(errorMessage);
            }
        });
    }

    /**
     * Displays the reservations returned by the API.
     *
     * @param reservations reservation list returned by the API
     */
    private void displayReservations(
            List<Reservation> reservations
    ) {

        reservationsContainer.removeAllViews();

        if (reservations == null
                || reservations.isEmpty()) {

            showNoReservations(
                    showingCurrent
                            ? "No current reservations."
                            : "No reservation history."
            );

            return;
        }

        tvNoReservations.setVisibility(View.GONE);

        for (Reservation reservation : reservations) {

            TextView reservationView =
                    createReservationView(reservation);

            reservationsContainer.addView(reservationView);
        }
    }

    /**
     * Creates a clickable reservation display card.
     *
     * @param reservation reservation data
     * @return text view containing reservation information
     */
    private TextView createReservationView(
            Reservation reservation
    ) {

        TextView textView =
                new TextView(this);

        String stationName =
                reservation.getStationName();

        String reservationTime =
                reservation.getReservationTime();

        String status =
                reservation.getStatus();

        String type =
                reservation.getType();

        if (stationName == null
                || stationName.isEmpty()) {

            stationName = "Station";
        }

        if (reservationTime == null
                || reservationTime.isEmpty()) {

            reservationTime = "";
        }

        if (status == null
                || status.isEmpty()) {

            status = "Unknown";
        }

        if (type == null
                || type.isEmpty()) {

            type = "Not specified";
        }

        String formattedDate =
                formatReservationDate(
                        reservationTime
                );

        String formattedTime =
                formatReservationTime(
                        reservationTime
                );

        StringBuilder displayBuilder =
                new StringBuilder();

        displayBuilder
                .append("Station: ")
                .append(stationName);

        if (!formattedDate.isEmpty()) {

            displayBuilder
                    .append("\nDate: ")
                    .append(formattedDate);
        }

        if (!formattedTime.isEmpty()) {

            displayBuilder
                    .append("\nTime: ")
                    .append(formattedTime);
        }

        displayBuilder
                .append("\nEnergy: ")
                .append(reservation.getEnergyKwh())
                .append(" kWh")
                .append("\nType: ")
                .append(type)
                .append("\nStatus: ")
                .append(status)
                .append("\n\nTap to view details");

        textView.setText(
                displayBuilder.toString()
        );

        textView.setTextSize(16);

        textView.setTextColor(
                Color.rgb(30, 30, 30)
        );

        textView.setPadding(
                32,
                28,
                32,
                28
        );

        textView.setGravity(
                Gravity.START
        );

        LinearLayout.LayoutParams params =
                new LinearLayout.LayoutParams(
                        LinearLayout.LayoutParams.MATCH_PARENT,
                        LinearLayout.LayoutParams.WRAP_CONTENT
                );

        params.setMargins(
                0,
                0,
                0,
                20
        );

        textView.setLayoutParams(params);

        textView.setBackgroundColor(
                Color.rgb(245, 247, 250)
        );

        /*
         * Opens the reservation details screen using
         * the reservation ID returned by the API.
         */
        textView.setOnClickListener(v -> {

            if (reservation.getId() == null
                    || reservation.getId().isEmpty()) {

                return;
            }

            Intent intent =
                    new Intent(
                            MyReservationsActivity.this,
                            ReservationDetailsActivity.class
                    );

            intent.putExtra(
                    "reservationId",
                    reservation.getId()
            );

            startActivity(intent);
        });

        return textView;
    }

    /**
     * Formats the reservation date for display, converting the API's
     * UTC value to the phone's local time zone.
     *
     * Example (Sri Lanka):
     * 2026-10-03T04:30:00Z
     * becomes:
     * 03 Oct 2026
     *
     * @param reservationTime API reservation date/time in UTC
     * @return formatted local reservation date
     */
    private String formatReservationDate(
            String reservationTime
    ) {

        return DateUtils.formatLocalDate(reservationTime);
    }

    /**
     * Formats the reservation time for display, converting the API's
     * UTC value to the phone's local time zone.
     *
     * Example (Sri Lanka):
     * 2026-10-03T04:30:00Z
     * becomes:
     * 10:00 AM
     *
     * @param reservationTime API reservation date/time in UTC
     * @return formatted local reservation time
     */
    private String formatReservationTime(
            String reservationTime
    ) {

        return DateUtils.formatLocalTime(reservationTime);
    }

    /**
     * Shows the loading indicator.
     */
    private void showLoading() {

        progressReservations.setVisibility(
                View.VISIBLE
        );

        tvNoReservations.setVisibility(
                View.GONE
        );

        reservationsContainer.removeAllViews();
    }

    /**
     * Hides the loading indicator.
     */
    private void hideLoading() {

        progressReservations.setVisibility(
                View.GONE
        );
    }

    /**
     * Displays an empty or error message.
     *
     * @param message message to display
     */
    private void showNoReservations(
            String message
    ) {

        reservationsContainer.removeAllViews();

        tvNoReservations.setText(
                message
        );

        tvNoReservations.setVisibility(
                View.VISIBLE
        );
    }
}