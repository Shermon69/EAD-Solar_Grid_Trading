/*
 * File:        QrScanActivity.java
 * Author:      WMVSB Wahundeniya (IT22292872)
 * Description: Scans a QR code and verifies it with the backend. 
 *              Allows grid operators to mark a reservation as complete.
 * Created:     29/09/2026
 */
package com.solargrid.app.activities;

import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.TextView;
import android.widget.Toast;

import androidx.activity.result.ActivityResultLauncher;
import androidx.appcompat.app.AppCompatActivity;

import com.journeyapps.barcodescanner.ScanContract;
import com.journeyapps.barcodescanner.ScanOptions;
import com.solargrid.app.R;
import com.solargrid.app.api.ApiClient;
import com.solargrid.app.models.QrVerifyRequest;

import com.solargrid.app.models.Reservation;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class QrScanActivity extends AppCompatActivity {
    private TextView tvScanResult;
    private Button btnScan;
    private Button btnComplete;
    private String scannedReservationId;

    private final ActivityResultLauncher<ScanOptions> barcodeLauncher = registerForActivityResult(new ScanContract(), result -> {
        if(result.getContents() == null) {
            Toast.makeText(QrScanActivity.this, "Scan cancelled", Toast.LENGTH_LONG).show();
        } else {
            verifyQrOnServer(result.getContents());
        }
    });

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_qr_scan);
        setTitle("Scan QR");

        tvScanResult = findViewById(R.id.tvScanResult);
        btnScan = findViewById(R.id.btnScan);
        btnComplete = findViewById(R.id.btnComplete);

        btnScan.setOnClickListener(v -> scanCode());
        btnComplete.setOnClickListener(v -> completeReservation());
    }

    /**
     * Executes the scanCode operation.
     */
    private void scanCode() {
        ScanOptions options = new ScanOptions();
        options.setPrompt("Volume up to flash on");
        options.setBeepEnabled(true);
        options.setOrientationLocked(true);
        options.setCaptureActivity(com.journeyapps.barcodescanner.CaptureActivity.class);
        barcodeLauncher.launch(options);
    }

    /**
     * Executes the verifyQrOnServer operation.
     */
    private void verifyQrOnServer(String qrText) {
        String[] parts = qrText.split("\\|");
        if (parts.length != 2) {
            tvScanResult.setText("Invalid QR Format.");
            btnComplete.setVisibility(View.GONE);
            return;
        }

        scannedReservationId = parts[0];
        String qrToken = parts[1];

        tvScanResult.setText("Verifying...");

        QrVerifyRequest request = new QrVerifyRequest(scannedReservationId, qrToken);
        ApiClient.getService(this).verifyQr(request).enqueue(new Callback<Reservation>() {
            @Override
            public void onResponse(Call<Reservation> call, Response<Reservation> response) {
                if (response.isSuccessful() && response.body() != null) {
                    Reservation r = response.body();
                    String details = String.format("Verification Successful!\nStation: %s\nTime: %s\nAmount: %.2f kWh\nProsumer NIC: %s", 
                        r.stationName != null ? r.stationName : r.stationId, r.scheduledDate, r.energyKwh, r.prosumerNic);
                    tvScanResult.setText(details);
                    btnComplete.setVisibility(View.VISIBLE);
                } else {
                    tvScanResult.setText("Verification Failed. Invalid or expired QR.");
                    btnComplete.setVisibility(View.GONE);
                }
            }

            @Override
            public void onFailure(Call<Reservation> call, Throwable t) {
                tvScanResult.setText("Network error: " + t.getMessage());
                btnComplete.setVisibility(View.GONE);
            }
        });
    }

    /**
     * Executes the completeReservation operation.
     */
    private void completeReservation() {
        if (scannedReservationId == null) return;
        
        btnComplete.setEnabled(false);
        ApiClient.getService(this).completeReservation(scannedReservationId).enqueue(new Callback<Object>() {
            @Override
            public void onResponse(Call<Object> call, Response<Object> response) {
                if (response.isSuccessful()) {
                    Toast.makeText(QrScanActivity.this, "Transfer Completed!", Toast.LENGTH_LONG).show();
                    finish();
                } else {
                    Toast.makeText(QrScanActivity.this, "Failed to complete.", Toast.LENGTH_LONG).show();
                    btnComplete.setEnabled(true);
                }
            }

            @Override
            public void onFailure(Call<Object> call, Throwable t) {
                Toast.makeText(QrScanActivity.this, "Network error", Toast.LENGTH_LONG).show();
                btnComplete.setEnabled(true);
            }
        });
    }
}
