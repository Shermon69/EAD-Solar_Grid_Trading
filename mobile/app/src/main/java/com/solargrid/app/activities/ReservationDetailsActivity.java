/*
 * File:        ReservationDetailsActivity.java
 * Author:      Dissanayake D.M.S.N (IT22210692)
 * Description: Displays details of a selected energy reservation and
 *              provides reservation management actions including QR display.
 * Created:     30/09/2026
 */

package com.solargrid.app.activities;

import android.content.Intent;
import android.graphics.Bitmap;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;

import com.google.zxing.BarcodeFormat;
import com.google.zxing.WriterException;
import com.google.zxing.common.BitMatrix;
import com.google.zxing.qrcode.QRCodeWriter;
import com.solargrid.app.R;
import com.solargrid.app.api.ApiClient;
import com.solargrid.app.api.ApiService;
import com.solargrid.app.models.Reservation;
import com.solargrid.app.utils.DateUtils;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

/**
 * Activity used to display details of an energy reservation.
 */
public class ReservationDetailsActivity extends AppCompatActivity {

    private TextView tvDetailStation;
    private TextView tvDetailDate;
    private TextView tvDetailEnergy;
    private TextView tvDetailType;
    private TextView tvDetailStatus;

    private TextView tvQrTitle;
    private TextView tvQrInfo;

    private ImageView ivReservationQr;

    private Button btnEditReservation;
    private Button btnCancelReservation;

    private ProgressBar progressReservationDetails;

    private ApiService apiService;

    private String reservationId = "";

    /**
     * Creates the reservation details screen.
     *
     * @param savedInstanceState previously saved activity state
     */
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        setContentView(R.layout.activity_reservation_details);

        initializeViews();

        apiService = ApiClient.getService(this);

        reservationId =
                getIntent().getStringExtra("reservationId");

        if (reservationId == null || reservationId.isEmpty()) {

            Toast.makeText(
                    this,
                    "Reservation ID was not provided.",
                    Toast.LENGTH_LONG
            ).show();

            finish();
        }
    }

    /**
     * Loads the reservation each time the screen is shown, so the details are
     * up to date after coming back from the edit or summary screen.
     */
    @Override
    protected void onResume() {
        super.onResume();

        if (reservationId != null && !reservationId.isEmpty()) {
            loadReservation();
        }
    }

    /**
     * Finds and stores references to the screen controls.
     */
    private void initializeViews() {

        tvDetailStation =
                findViewById(R.id.tvDetailStation);

        tvDetailDate =
                findViewById(R.id.tvDetailDate);

        tvDetailEnergy =
                findViewById(R.id.tvDetailEnergy);

        tvDetailType =
                findViewById(R.id.tvDetailType);

        tvDetailStatus =
                findViewById(R.id.tvDetailStatus);

        tvQrTitle =
                findViewById(R.id.tvQrTitle);

        tvQrInfo =
                findViewById(R.id.tvQrInfo);

        ivReservationQr =
                findViewById(R.id.ivReservationQr);

        btnEditReservation =
                findViewById(R.id.btnEditReservation);

        btnCancelReservation =
                findViewById(R.id.btnCancelReservation);

        progressReservationDetails =
                findViewById(R.id.progressReservationDetails);
    }

    /**
     * Loads the selected reservation from the central Web API.
     */
    private void loadReservation() {

        showLoading();

        apiService.getReservation(
                reservationId
        ).enqueue(new Callback<Reservation>() {

            @Override
            public void onResponse(
                    Call<Reservation> call,
                    Response<Reservation> response
            ) {

                hideLoading();

                if (!response.isSuccessful()
                        || response.body() == null) {

                    Toast.makeText(
                            ReservationDetailsActivity.this,
                            "Unable to load reservation details.",
                            Toast.LENGTH_LONG
                    ).show();

                    return;
                }

                displayReservation(response.body());
            }

            @Override
            public void onFailure(
                    Call<Reservation> call,
                    Throwable t
            ) {

                hideLoading();

                Toast.makeText(
                        ReservationDetailsActivity.this,
                        "Unable to connect to server.",
                        Toast.LENGTH_LONG
                ).show();
            }
        });
    }

    /**
     * Displays reservation information on the screen.
     *
     * @param reservation reservation returned by the API
     */
    private void displayReservation(
            Reservation reservation
    ) {

        String stationName =
                reservation.getStationName();

        String reservationTime =
                reservation.getReservationTime();

        String type =
                reservation.getType();

        String status =
                reservation.getStatus();

        if (stationName == null || stationName.isEmpty()) {
            stationName = "Station not available";
        }

        if (reservationTime == null || reservationTime.isEmpty()) {
            reservationTime = "Date not available";
        }

        if (type == null || type.isEmpty()) {
            type = "Not specified";
        }

        if (status == null || status.isEmpty()) {
            status = "Unknown";
        }

        tvDetailStation.setText(stationName);

        tvDetailDate.setText(
                formatReservationDate(reservationTime)
        );

        tvDetailEnergy.setText(
                reservation.getEnergyKwh() + " kWh"
        );

        tvDetailType.setText(type);

        tvDetailStatus.setText(status);

        displayQrInformation(
                reservation.getQrToken()
        );

        configureActionButtons(status);
    }

    /**
     * Generates and displays the reservation QR code when
     * the reservation has an approved QR token.
     *
     * @param qrToken QR token returned by the API
     */
    private void displayQrInformation(String qrToken) {

        if (qrToken == null || qrToken.trim().isEmpty()) {

            tvQrTitle.setVisibility(View.GONE);

            ivReservationQr.setVisibility(View.GONE);

            tvQrInfo.setVisibility(View.GONE);

            return;
        }

        /*
         * QR content format:
         *
         * reservationId|qrToken
         *
         * This is the value that will be scanned by the
         * grid operator's QR verification process.
         */
        String qrContent =
                reservationId + "|" + qrToken;

        Bitmap qrBitmap =
                generateQrCode(qrContent);

        if (qrBitmap == null) {

            tvQrTitle.setVisibility(View.VISIBLE);

            ivReservationQr.setVisibility(View.GONE);

            tvQrInfo.setVisibility(View.VISIBLE);

            tvQrInfo.setText(
                    "Unable to generate the reservation QR code."
            );

            return;
        }

        tvQrTitle.setVisibility(View.VISIBLE);

        ivReservationQr.setVisibility(View.VISIBLE);

        tvQrInfo.setVisibility(View.VISIBLE);

        ivReservationQr.setImageBitmap(qrBitmap);

        tvQrInfo.setText(
                "Show this QR code to the grid operator " +
                        "when completing your reservation."
        );
    }

    /**
     * Generates a QR code bitmap using ZXing.
     *
     * @param content text encoded inside the QR code
     * @return generated QR bitmap, or null if generation fails
     */
    private Bitmap generateQrCode(String content) {

        try {

            QRCodeWriter writer =
                    new QRCodeWriter();

            BitMatrix bitMatrix =
                    writer.encode(
                            content,
                            BarcodeFormat.QR_CODE,
                            600,
                            600
                    );

            int width =
                    bitMatrix.getWidth();

            int height =
                    bitMatrix.getHeight();

            Bitmap bitmap =
                    Bitmap.createBitmap(
                            width,
                            height,
                            Bitmap.Config.ARGB_8888
                    );

            for (int x = 0; x < width; x++) {

                for (int y = 0; y < height; y++) {

                    bitmap.setPixel(
                            x,
                            y,
                            bitMatrix.get(x, y)
                                    ? android.graphics.Color.BLACK
                                    : android.graphics.Color.WHITE
                    );
                }
            }

            return bitmap;

        } catch (WriterException e) {

            return null;
        }
    }

    /**
     * Enables or disables reservation actions based on status.
     *
     * @param status current reservation status
     */
    private void configureActionButtons(String status) {

        boolean canModify =
                "Pending".equalsIgnoreCase(status)
                        || "Approved".equalsIgnoreCase(status);

        btnEditReservation.setEnabled(canModify);

        btnCancelReservation.setEnabled(canModify);

        /*
         * Opens the edit reservation screen.
         */
        btnEditReservation.setOnClickListener(v -> {

            Intent intent =
                    new Intent(
                            ReservationDetailsActivity.this,
                            EditReservationActivity.class
                    );

            intent.putExtra(
                    "reservationId",
                    reservationId
            );

            startActivity(intent);
        });

        /*
         * Opens the cancellation confirmation dialog.
         */
        btnCancelReservation.setOnClickListener(v ->
                showCancelConfirmation()
        );
    }

    /**
     * Displays a confirmation dialog before cancelling a reservation.
     */
    private void showCancelConfirmation() {

        new AlertDialog.Builder(this)
                .setTitle("Cancel Reservation")
                .setMessage(
                        "Are you sure you want to cancel this reservation?"
                )
                .setNegativeButton(
                        "No",
                        null
                )
                .setPositiveButton(
                        "Yes, Cancel",
                        (dialog, which) ->
                                cancelReservation()
                )
                .show();
    }

    /**
     * Sends the cancellation request to the central Web API.
     */
    private void cancelReservation() {

        showLoading();

        apiService.cancelReservation(
                reservationId
        ).enqueue(new Callback<Reservation>() {

            @Override
            public void onResponse(
                    Call<Reservation> call,
                    Response<Reservation> response
            ) {

                hideLoading();

                if (response.isSuccessful()
                        && response.body() != null) {

                    Reservation updatedReservation =
                            response.body();

                    tvDetailStatus.setText("Cancelled");

                    btnCancelReservation.setEnabled(false);

                    btnEditReservation.setEnabled(false);

                    displayQrInformation(
                            updatedReservation.getQrToken()
                    );

                    // Show the summary page for the cancelled booking
                    BookingSummaryActivity.open(
                            ReservationDetailsActivity.this,
                            BookingSummaryActivity.ACTION_CANCELLED,
                            updatedReservation
                    );

                } else {

                    Toast.makeText(
                            ReservationDetailsActivity.this,
                            "Unable to cancel reservation.",
                            Toast.LENGTH_LONG
                    ).show();
                }
            }

            @Override
            public void onFailure(
                    Call<Reservation> call,
                    Throwable t
            ) {

                hideLoading();

                Toast.makeText(
                        ReservationDetailsActivity.this,
                        "Unable to connect to server.",
                        Toast.LENGTH_LONG
                ).show();
            }
        });
    }

    /**
     * Formats the reservation date and time for display, converting the
     * API's UTC value to the phone's local time zone.
     *
     * Example (Sri Lanka):
     * 2026-10-03T02:30:00Z becomes 03 Oct 2026, 08:00 AM
     *
     * @param reservationTime API reservation date/time in UTC
     * @return formatted local reservation date/time
     */
    private String formatReservationDate(
            String reservationTime
    ) {

        return DateUtils.formatLocal(reservationTime);
    }

    /**
     * Shows the loading indicator.
     */
    private void showLoading() {

        progressReservationDetails.setVisibility(
                View.VISIBLE
        );

        btnEditReservation.setEnabled(false);

        btnCancelReservation.setEnabled(false);
    }

    /**
     * Hides the loading indicator.
     */
    private void hideLoading() {

        progressReservationDetails.setVisibility(
                View.GONE
        );
    }
}