/*
 * File:        BookingSummaryActivity.java
 * Author:      Shermon H (IT22177964)
 * Description: Summary page shown after a booking is created, updated or
 *              cancelled. It displays the booking exactly as the Web API
 *              returned it, so the prosumer can confirm what was saved.
 * Created:     05/10/2026
 */

package com.solargrid.app.activities;

import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.TextView;

import androidx.appcompat.app.AppCompatActivity;

import com.solargrid.app.R;
import com.solargrid.app.models.Reservation;
import com.solargrid.app.utils.DateUtils;

/**
 * Shows a summary of a booking after each booking action.
 */
public class BookingSummaryActivity extends AppCompatActivity {

    /** Action values passed in the intent. */
    public static final String ACTION_CREATED = "created";
    public static final String ACTION_UPDATED = "updated";
    public static final String ACTION_CANCELLED = "cancelled";

    private static final String EXTRA_ACTION = "action";
    private static final String EXTRA_ID = "reservationId";
    private static final String EXTRA_STATION = "stationName";
    private static final String EXTRA_TIME = "reservationTime";
    private static final String EXTRA_TYPE = "type";
    private static final String EXTRA_ENERGY = "energyKwh";
    private static final String EXTRA_STATUS = "status";

    private String reservationId;

    /**
     * Opens the summary page for a booking returned by the API.
     *
     * @param context     the screen that performed the action
     * @param action      ACTION_CREATED, ACTION_UPDATED or ACTION_CANCELLED
     * @param reservation the reservation returned by the API
     */
    public static void open(Context context, String action, Reservation reservation) {
        Intent intent = new Intent(context, BookingSummaryActivity.class);
        intent.putExtra(EXTRA_ACTION, action);
        intent.putExtra(EXTRA_ID, reservation.getId());
        intent.putExtra(EXTRA_STATION, reservation.getStationName());
        intent.putExtra(EXTRA_TIME, reservation.getReservationTime());
        intent.putExtra(EXTRA_TYPE, reservation.getType());
        intent.putExtra(EXTRA_ENERGY, reservation.getEnergyKwh());
        intent.putExtra(EXTRA_STATUS, reservation.getStatus());
        context.startActivity(intent);
    }

    /**
     * Fills the summary page from the intent extras and sets up the buttons.
     *
     * @param savedInstanceState previously saved activity state
     */
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_booking_summary);

        Intent intent = getIntent();
        String action = intent.getStringExtra(EXTRA_ACTION);
        reservationId = intent.getStringExtra(EXTRA_ID);

        showActionText(action);

        setText(R.id.tvSummaryReference, reservationId);
        setText(R.id.tvSummaryStation, intent.getStringExtra(EXTRA_STATION));
        setText(R.id.tvSummaryDate, DateUtils.formatLocal(intent.getStringExtra(EXTRA_TIME)));
        setText(R.id.tvSummaryType, intent.getStringExtra(EXTRA_TYPE));
        setText(R.id.tvSummaryEnergy, intent.getDoubleExtra(EXTRA_ENERGY, 0) + " kWh");
        setText(R.id.tvSummaryStatus, intent.getStringExtra(EXTRA_STATUS));

        Button btnViewBooking = findViewById(R.id.btnSummaryViewBooking);
        btnViewBooking.setOnClickListener(v -> openBookingDetails());

        // A cancelled booking cannot be changed any more, so there is nothing to view
        if (ACTION_CANCELLED.equals(action)) {
            btnViewBooking.setVisibility(View.GONE);
        }

        Button btnDone = findViewById(R.id.btnSummaryDone);
        btnDone.setOnClickListener(v -> finish());
    }

    /**
     * Sets the title, message and next-step text for the action that was done.
     *
     * @param action ACTION_CREATED, ACTION_UPDATED or ACTION_CANCELLED
     */
    private void showActionText(String action) {
        String title;
        String message;
        String nextStep;

        if (ACTION_UPDATED.equals(action)) {
            title = "Booking Updated";
            message = "Your booking was updated successfully.";
            nextStep = "You can change or cancel this booking up to 12 hours before it starts.";
        } else if (ACTION_CANCELLED.equals(action)) {
            title = "Booking Cancelled";
            message = "Your booking was cancelled successfully.";
            nextStep = "The battery slot has been released for other prosumers.";
        } else {
            title = "Booking Created";
            message = "Your booking was created successfully.";
            nextStep = "A grid operator will review your booking. "
                    + "Once it is approved, its QR code will appear on the booking details screen.";
        }

        setTitle(title);
        setText(R.id.tvSummaryTitle, title);
        setText(R.id.tvSummaryMessage, message);
        setText(R.id.tvSummaryNextStep, nextStep);
    }

    /**
     * Opens the booking details screen. If the details screen is already open
     * underneath (after an update), it is brought back instead of opened twice.
     */
    private void openBookingDetails() {
        Intent intent = new Intent(this, ReservationDetailsActivity.class);
        intent.putExtra("reservationId", reservationId);
        intent.addFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP | Intent.FLAG_ACTIVITY_SINGLE_TOP);
        startActivity(intent);
        finish();
    }

    /**
     * Shows a value in a TextView, or "-" when the API did not return it.
     *
     * @param viewId the TextView ID
     * @param value  the value to show
     */
    private void setText(int viewId, String value) {
        TextView textView = findViewById(viewId);
        textView.setText(value == null || value.trim().isEmpty() ? "-" : value);
    }
}
