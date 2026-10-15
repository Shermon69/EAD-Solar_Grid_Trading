/*
 * File:        QrScanActivity.java
 * Author:      WMVSB Wahundeniya (IT22292872)
 * Description: Operator mode QR scanner. Shows a live portrait camera preview,
 *              reads the "reservationId|qrToken" QR code from the prosumer's
 *              booking screen, verifies it with the Web API (rule R9) and lets
 *              the grid operator mark the energy transfer as Completed.
 */
package com.solargrid.app.activities;

import android.Manifest;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.content.res.ColorStateList;
import android.net.Uri;
import android.os.Bundle;
import android.provider.Settings;
import android.view.LayoutInflater;
import android.view.View;
import android.widget.ImageButton;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.TextView;

import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.contract.ActivityResultContracts;
import androidx.annotation.NonNull;
import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.content.ContextCompat;
import androidx.core.widget.NestedScrollView;

import com.google.android.material.button.MaterialButton;
import com.google.android.material.card.MaterialCardView;
import com.google.android.material.progressindicator.LinearProgressIndicator;
import com.google.zxing.BarcodeFormat;
import com.journeyapps.barcodescanner.BarcodeResult;
import com.journeyapps.barcodescanner.DecoratedBarcodeView;
import com.journeyapps.barcodescanner.DefaultDecoderFactory;
import com.google.zxing.client.android.BeepManager;
import com.solargrid.app.R;
import com.solargrid.app.api.ApiClient;
import com.solargrid.app.models.QrVerifyRequest;
import com.solargrid.app.models.Reservation;
import com.solargrid.app.utils.ApiErrorUtils;
import com.solargrid.app.utils.DateUtils;

import java.util.Collections;
import java.util.Locale;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

/**
 * Grid operator screen for scanning and completing reservations.
 */
public class QrScanActivity extends AppCompatActivity {

    // Screen states, so the camera, result card and buttons always match what is
    // happening
    private static final int STATE_SCANNING = 0;
    private static final int STATE_VERIFYING = 1;
    private static final int STATE_VERIFIED = 2;
    private static final int STATE_FAILED = 3;
    private static final int STATE_COMPLETED = 4;

    private DecoratedBarcodeView barcodeScanner;
    private View layoutScannerPaused;
    private TextView tvScannerPaused;
    private MaterialButton btnGrantPermission;
    private ImageButton btnTorch;
    private TextView tvScanHint;
    private NestedScrollView scrollView;

    private MaterialCardView cardResult;
    private ImageView ivResultIcon;
    private TextView tvResultTitle;
    private TextView tvResultMessage;
    private LinearProgressIndicator progressVerify;
    private LinearLayout layoutDetails;
    private MaterialButton btnComplete;
    private MaterialButton btnScanAgain;

    private BeepManager beepManager;
    private int state = STATE_SCANNING;
    private boolean torchOn = false;
    private String scannedReservationId;

    // Asks Android for camera permission and starts scanning if it is granted
    private final ActivityResultLauncher<String> cameraPermissionLauncher = registerForActivityResult(
            new ActivityResultContracts.RequestPermission(), granted -> {
                if (granted) {
                    showState(STATE_SCANNING);
                } else {
                    showPermissionMessage();
                }
            });

    /**
     * Links the views, sets up the scanner and asks for camera permission if
     * needed.
     */
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_qr_scan);

        ((TextView) findViewById(R.id.tvHeaderTitle)).setText(R.string.qr_title);
        ((TextView) findViewById(R.id.tvHeaderSubtitle)).setText(R.string.qr_subtitle);
        findViewById(R.id.btnHeaderBack).setOnClickListener(v -> finish());

        barcodeScanner = findViewById(R.id.barcodeScanner);
        layoutScannerPaused = findViewById(R.id.layoutScannerPaused);
        tvScannerPaused = findViewById(R.id.tvScannerPaused);
        btnGrantPermission = findViewById(R.id.btnGrantPermission);
        btnTorch = findViewById(R.id.btnTorch);
        tvScanHint = findViewById(R.id.tvScanHint);
        scrollView = findViewById(R.id.scrollView);
        cardResult = findViewById(R.id.cardResult);
        ivResultIcon = findViewById(R.id.ivResultIcon);
        tvResultTitle = findViewById(R.id.tvResultTitle);
        tvResultMessage = findViewById(R.id.tvResultMessage);
        progressVerify = findViewById(R.id.progressVerify);
        layoutDetails = findViewById(R.id.layoutDetails);
        btnComplete = findViewById(R.id.btnComplete);
        btnScanAgain = findViewById(R.id.btnScanAgain);

        setupScanner();

        btnComplete.setOnClickListener(v -> confirmComplete());
        btnScanAgain.setOnClickListener(v -> showState(STATE_SCANNING));
        btnGrantPermission.setOnClickListener(v -> requestCameraPermission());

        if (hasCameraPermission()) {
            showState(STATE_SCANNING);
        } else {
            requestCameraPermission();
        }
    }

    /**
     * Configures the embedded scanner: QR codes only, continuous focus,
     * a beep on success and the flashlight button.
     */
    private void setupScanner() {
        barcodeScanner.setDecoderFactory(
                new DefaultDecoderFactory(Collections.singletonList(BarcodeFormat.QR_CODE)));
        barcodeScanner.getBarcodeView().getCameraSettings().setContinuousFocusEnabled(true);
        barcodeScanner.setStatusText("");

        beepManager = new BeepManager(this);

        // Hide the flashlight button on phones without a flash
        if (!getPackageManager().hasSystemFeature(PackageManager.FEATURE_CAMERA_FLASH)) {
            btnTorch.setVisibility(View.GONE);
        }
        barcodeScanner.setTorchListener(new DecoratedBarcodeView.TorchListener() {
            @Override
            public void onTorchOn() {
                torchOn = true;
                btnTorch.setImageResource(R.drawable.ic_flash_on);
            }

            @Override
            public void onTorchOff() {
                torchOn = false;
                btnTorch.setImageResource(R.drawable.ic_flash_off);
            }
        });
        btnTorch.setOnClickListener(v -> {
            if (torchOn) {
                barcodeScanner.setTorchOff();
            } else {
                barcodeScanner.setTorchOn();
            }
        });
    }

    /**
     * Restarts the camera when the operator comes back to this screen while
     * scanning.
     */
    @Override
    protected void onResume() {
        super.onResume();
        if (state == STATE_SCANNING && hasCameraPermission()) {
            if (btnGrantPermission.getVisibility() == View.VISIBLE) {
                // Permission was just allowed from the app settings screen
                showState(STATE_SCANNING);
            } else {
                barcodeScanner.resume();
            }
        }
    }

    /**
     * Releases the camera whenever the screen is not visible.
     */
    @Override
    protected void onPause() {
        super.onPause();
        barcodeScanner.pause();
    }

    /**
     * Returns true if the app is allowed to use the camera.
     */
    private boolean hasCameraPermission() {
        return ContextCompat.checkSelfPermission(this, Manifest.permission.CAMERA) == PackageManager.PERMISSION_GRANTED;
    }

    /**
     * Shows the system permission dialog, or opens the app settings if the
     * operator chose "Don't ask again" earlier.
     */
    private void requestCameraPermission() {
        if (btnGrantPermission.getVisibility() == View.VISIBLE
                && !shouldShowRequestPermissionRationale(Manifest.permission.CAMERA)) {
            Intent intent = new Intent(Settings.ACTION_APPLICATION_DETAILS_SETTINGS,
                    Uri.fromParts("package", getPackageName(), null));
            startActivity(intent);
            return;
        }
        cameraPermissionLauncher.launch(Manifest.permission.CAMERA);
    }

    /**
     * Covers the camera card with a message and an "Allow camera" button.
     */
    private void showPermissionMessage() {
        barcodeScanner.pause();
        layoutScannerPaused.setVisibility(View.VISIBLE);
        tvScannerPaused.setText(R.string.qr_permission_needed);
        btnGrantPermission.setVisibility(View.VISIBLE);
        btnTorch.setVisibility(View.GONE);
    }

    /**
     * Switches the screen to the given state: starts or pauses the camera
     * and shows the matching result card, icon and buttons.
     */
    private void showState(int newState) {
        state = newState;
        btnGrantPermission.setVisibility(View.GONE);

        if (newState == STATE_SCANNING) {
            scannedReservationId = null;
            layoutScannerPaused.setVisibility(View.GONE);
            cardResult.setVisibility(View.GONE);
            tvScanHint.setText(R.string.qr_hint);
            if (getPackageManager().hasSystemFeature(PackageManager.FEATURE_CAMERA_FLASH)) {
                btnTorch.setVisibility(View.VISIBLE);
            }
            barcodeScanner.resume();
            barcodeScanner.decodeSingle(this::onQrScanned);
            return;
        }

        // Every other state pauses the camera and shows the result card
        barcodeScanner.setTorchOff();
        barcodeScanner.pause();
        btnTorch.setVisibility(View.GONE);
        layoutScannerPaused.setVisibility(View.VISIBLE);
        cardResult.setVisibility(View.VISIBLE);
        progressVerify.setVisibility(newState == STATE_VERIFYING ? View.VISIBLE : View.GONE);
        btnComplete.setVisibility(newState == STATE_VERIFIED ? View.VISIBLE : View.GONE);
        btnComplete.setEnabled(true);
        btnComplete.setText(R.string.qr_complete);
        btnScanAgain.setVisibility(newState == STATE_VERIFYING ? View.GONE : View.VISIBLE);

        switch (newState) {
            case STATE_VERIFYING:
                tvScannerPaused.setText(R.string.qr_verifying);
                setResultHeader(R.drawable.ic_qr_scan, R.color.solar, R.color.solar_light,
                        getString(R.string.qr_verifying), "");
                layoutDetails.removeAllViews();
                break;
            case STATE_VERIFIED:
                tvScannerPaused.setText(R.string.qr_verified);
                break;
            case STATE_FAILED:
                tvScannerPaused.setText(R.string.qr_failed);
                break;
            case STATE_COMPLETED:
                tvScannerPaused.setText(R.string.qr_completed);
                break;
            default:
                break;
        }
        tvScanHint.setText("");
        scrollView.post(() -> scrollView.smoothScrollTo(0, cardResult.getTop()));
    }

    /**
     * Called by the scanner when a QR code is read. Checks the code has the
     * "reservationId|qrToken" format and sends it to the API for verification.
     */
    private void onQrScanned(BarcodeResult result) {
        beepManager.playBeepSoundAndVibrate();

        String text = result.getText() == null ? "" : result.getText().trim();
        String[] parts = text.split("\\|");
        if (parts.length != 2 || parts[0].trim().isEmpty() || parts[1].trim().isEmpty()) {
            showFailure(getString(R.string.qr_invalid_format));
            return;
        }

        scannedReservationId = parts[0].trim();
        verifyQrOnServer(scannedReservationId, parts[1].trim());
    }

    /**
     * Sends the reservation ID and QR token to POST /api/reservations/verify-qr.
     * The API checks the booking exists, is Approved and the token matches.
     */
    private void verifyQrOnServer(String reservationId, String qrToken) {
        showState(STATE_VERIFYING);

        QrVerifyRequest request = new QrVerifyRequest(reservationId, qrToken);
        ApiClient.getService(this).verifyQr(request).enqueue(new Callback<Reservation>() {
            @Override
            public void onResponse(@NonNull Call<Reservation> call,
                    @NonNull Response<Reservation> response) {
                if (isFinishing()) {
                    return;
                }
                if (response.isSuccessful() && response.body() != null) {
                    showVerified(response.body());
                } else {
                    showFailure(ApiErrorUtils.getMessage(response));
                }
            }

            @Override
            public void onFailure(@NonNull Call<Reservation> call, @NonNull Throwable t) {
                if (!isFinishing()) {
                    showFailure(ApiErrorUtils.getNetworkMessage(t));
                }
            }
        });
    }

    /**
     * Shows the verified reservation and the "Complete energy transfer" button.
     */
    private void showVerified(Reservation r) {
        showState(STATE_VERIFIED);
        setResultHeader(R.drawable.ic_check_circle, R.color.success, R.color.success_light,
                getString(R.string.qr_verified), "Check the details with the prosumer, then complete the transfer.");
        fillReservationDetails(r);
    }

    /**
     * Shows why the QR code could not be verified, with a "Scan another code"
     * button.
     */
    private void showFailure(String message) {
        showState(STATE_FAILED);
        setResultHeader(R.drawable.ic_error, R.color.danger, R.color.danger_light,
                getString(R.string.qr_failed), message);
        layoutDetails.removeAllViews();
    }

    /**
     * Asks the operator to confirm before the reservation is completed,
     * because a completed reservation cannot be changed back.
     */
    private void confirmComplete() {
        new AlertDialog.Builder(this)
                .setTitle(R.string.qr_complete_confirm_title)
                .setMessage(R.string.qr_complete_confirm_msg)
                .setPositiveButton(R.string.qr_complete, (dialog, which) -> completeReservation())
                .setNegativeButton(android.R.string.cancel, null)
                .show();
    }

    /**
     * Sends PATCH /api/reservations/{id}/complete. The API sets the status to
     * Completed and records which operator finished the transfer.
     */
    private void completeReservation() {
        if (scannedReservationId == null) {
            return;
        }

        btnComplete.setEnabled(false);
        btnComplete.setText(R.string.qr_completing);
        ApiClient.getService(this).completeReservation(scannedReservationId)
                .enqueue(new Callback<Reservation>() {
                    @Override
                    public void onResponse(@NonNull Call<Reservation> call,
                            @NonNull Response<Reservation> response) {
                        if (isFinishing()) {
                            return;
                        }
                        if (response.isSuccessful() && response.body() != null) {
                            showCompleted(response.body());
                        } else {
                            showFailure(ApiErrorUtils.getMessage(response));
                        }
                    }

                    @Override
                    public void onFailure(@NonNull Call<Reservation> call, @NonNull Throwable t) {
                        if (isFinishing()) {
                            return;
                        }
                        // Keep the verified details so the operator can simply try again
                        btnComplete.setEnabled(true);
                        btnComplete.setText(R.string.qr_complete);
                        tvResultMessage.setText(ApiErrorUtils.getNetworkMessage(t));
                    }
                });
    }

    /**
     * Shows the completed reservation, including when and by whom it was completed.
     */
    private void showCompleted(Reservation r) {
        showState(STATE_COMPLETED);
        setResultHeader(R.drawable.ic_check_circle, R.color.success, R.color.success_light,
                getString(R.string.qr_completed), "The reservation has been marked as Completed.");
        fillReservationDetails(r);
        if (r.getCompletedAt() != null) {
            addDetailRow("Completed at", DateUtils.formatLocal(r.getCompletedAt()));
        }
        if (r.getCompletedBy() != null) {
            addDetailRow("Completed by", r.getCompletedBy());
        }
    }

    /**
     * Sets the icon, colours, title and message at the top of the result card.
     */
    private void setResultHeader(int iconRes, int colorRes, int backgroundRes,
            String title, String message) {
        ivResultIcon.setImageResource(iconRes);
        ivResultIcon.setImageTintList(ColorStateList.valueOf(ContextCompat.getColor(this, colorRes)));
        ivResultIcon.setBackgroundTintList(
                ColorStateList.valueOf(ContextCompat.getColor(this, backgroundRes)));
        tvResultTitle.setText(title);
        tvResultTitle.setTextColor(ContextCompat.getColor(this,
                colorRes == R.color.solar ? R.color.text_primary : colorRes));
        tvResultMessage.setText(message);
        tvResultMessage.setVisibility(message == null || message.isEmpty() ? View.GONE : View.VISIBLE);
    }

    /**
     * Lists the main reservation details returned by the API.
     */
    private void fillReservationDetails(Reservation r) {
        layoutDetails.removeAllViews();
        addDetailRow("Prosumer NIC", r.getProsumerNic());
        addDetailRow("Station", r.getStationName() != null ? r.getStationName() : r.getStationId());
        addDetailRow("Time slot", DateUtils.formatLocal(r.getReservationTime()));
        addDetailRow("Type", r.getType());
        addDetailRow("Energy", String.format(Locale.ENGLISH, "%.2f kWh", r.getEnergyKwh()));
        addDetailRow("Status", r.getStatus());
        addDetailRow("Reservation ID", r.getId());
    }

    /**
     * Adds one "label: value" row to the result card.
     */
    private void addDetailRow(String label, String value) {
        View row = LayoutInflater.from(this).inflate(R.layout.item_detail_row, layoutDetails, false);
        ((TextView) row.findViewById(R.id.tvRowLabel)).setText(label);
        ((TextView) row.findViewById(R.id.tvRowValue)).setText(value == null ? "-" : value);
        layoutDetails.addView(row);
    }
}
