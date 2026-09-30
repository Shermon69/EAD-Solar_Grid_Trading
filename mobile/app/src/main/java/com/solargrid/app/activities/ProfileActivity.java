/*
 * File:        ProfileActivity.java
 * Author:      Premaratne R.A.N.C (IT22050908)
 * Description: Edit Profile and Request Deactivation screen for the
 *              logged-in prosumer.
 * Created:     29/09/2026
 */
package com.solargrid.app.activities;

import android.os.Bundle;
import android.util.Patterns;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;

import com.solargrid.app.R;
import com.solargrid.app.api.ApiClient;
import com.solargrid.app.api.ApiService;
import com.solargrid.app.models.ApiMessage;
import com.solargrid.app.models.Prosumer;
import com.solargrid.app.models.UpdateProfileRequest;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

/**
 * Profile screen for the logged-in prosumer. Allows editing the profile
 * fields and requesting account deactivation (which requires Backoffice
 * approval before it takes effect).
 */
public class ProfileActivity extends AppCompatActivity {

    private EditText etName, etEmail, etPhone, etAddress, etNewPassword;
    private TextView tvNic, tvStatus;
    private ApiService api;

    /**
     * Sets up the screen, builds the API client with the saved JWT and
     * loads the current profile from the server.
     */
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_profile);

        api = ApiClient.getService(this);

        // Bind views.
        tvNic          = findViewById(R.id.tvNic);
        tvStatus       = findViewById(R.id.tvStatus);
        etName         = findViewById(R.id.etName);
        etEmail        = findViewById(R.id.etEmail);
        etPhone        = findViewById(R.id.etPhone);
        etAddress      = findViewById(R.id.etAddress);
        etNewPassword  = findViewById(R.id.etNewPassword);

        // Button handlers.
        ((Button) findViewById(R.id.btnSave))
                .setOnClickListener(v -> saveProfile());

        ((Button) findViewById(R.id.btnDeactivate))
                .setOnClickListener(v -> confirmDeactivation());

        // Fetch existing profile data.
        loadProfile();
    }

    /**
     * Loads the profile from the API and fills the form fields.
     * Also shows the NIC and status (including any pending deactivation request).
     */
    private void loadProfile() {
        api.getProfile().enqueue(new Callback<Prosumer>() {

            /** Fills the form with the profile data returned by the API. */
            @Override
            public void onResponse(Call<Prosumer> call, Response<Prosumer> response) {
                if (!response.isSuccessful() || response.body() == null) {
                    toast("Could not load profile.");
                    return;
                }

                Prosumer p = response.body();
                tvNic.setText("NIC: " + p.nic);
                tvStatus.setText("Status: " + p.status
                        + (p.deactivationRequested ? " (deactivation requested)" : ""));

                etName.setText(p.fullName);
                etEmail.setText(p.email);
                etPhone.setText(p.phone);
                etAddress.setText(p.address);
            }

            /** Handles network errors. */
            @Override
            public void onFailure(Call<Prosumer> call, Throwable t) {
                toast("Cannot reach the server.");
            }
        });
    }

    /**
     * Validates and saves the edited profile. The new password is optional —
     * if left blank, the old password is kept.
     */
    private void saveProfile() {
        String name    = etName.getText().toString().trim();
        String email   = etEmail.getText().toString().trim();
        String phone   = etPhone.getText().toString().trim();
        String address = etAddress.getText().toString().trim();
        String pw      = etNewPassword.getText().toString();

        // Field validation (mirrors the API DTO rules).
        if (name.length() < 3) {
            etName.setError("Enter your full name");
            return;
        }
        if (!Patterns.EMAIL_ADDRESS.matcher(email).matches()) {
            etEmail.setError("Enter a valid email");
            return;
        }
        if (!phone.matches("^0[0-9]{9}$")) {
            etPhone.setError("Phone must be 10 digits, starting with 0");
            return;
        }
        if (address.isEmpty()) {
            etAddress.setError("Enter your address");
            return;
        }
        if (!pw.isEmpty() && pw.length() < 8) {
            etNewPassword.setError("At least 8 characters");
            return;
        }

        // Send update to the API. Empty password means "keep the old one".
        api.updateProfile(new UpdateProfileRequest(
                        name, email, phone, address,
                        pw.isEmpty() ? null : pw))
                .enqueue(new Callback<ApiMessage>() {

                    /** Shows the result and clears the password field on success. */
                    @Override
                    public void onResponse(Call<ApiMessage> call, Response<ApiMessage> response) {
                        toast(response.isSuccessful()
                                ? "Profile updated."
                                : "Could not update profile.");

                        if (response.isSuccessful()) etNewPassword.setText("");
                    }

                    /** Handles network errors. */
                    @Override
                    public void onFailure(Call<ApiMessage> call, Throwable t) {
                        toast("Cannot reach the server.");
                    }
                });
    }

    /**
     * Shows a confirmation dialog before sending a deactivation request.
     * The actual account is not deactivated until Backoffice approves.
     */
    private void confirmDeactivation() {
        new AlertDialog.Builder(this)
                .setTitle("Request deactivation")
                .setMessage("Backoffice will review your request. Continue?")
                .setPositiveButton("Send request", (d, w) -> requestDeactivation())
                .setNegativeButton("Cancel", null)
                .show();
    }

    /**
     * Sends the deactivation request to the API. On success, reloads the
     * profile so the status line reflects the pending request.
     */
    private void requestDeactivation() {
        api.requestDeactivation().enqueue(new Callback<ApiMessage>() {

            /** Shows the result and refreshes the profile if successful. */
            @Override
            public void onResponse(Call<ApiMessage> call, Response<ApiMessage> response) {
                toast(response.isSuccessful()
                        ? "Deactivation requested."
                        : "Only active accounts can request deactivation.");

                if (response.isSuccessful()) loadProfile();
            }

            /** Handles network errors. */
            @Override
            public void onFailure(Call<ApiMessage> call, Throwable t) {
                toast("Cannot reach the server.");
            }
        });
    }

    /**
     * Small helper to show a long-duration Toast message.
     *
     * @param msg message to display
     */
    private void toast(String msg) {
        Toast.makeText(this, msg, Toast.LENGTH_LONG).show();
    }
}