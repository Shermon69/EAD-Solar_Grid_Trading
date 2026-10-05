/*
 * File:        RegisterActivity.java
 * Author:      Premaratne R.A.N.C (IT22050908)
 * Description: Prosumer registration screen. NIC is the primary key. Account
 *              stays Pending until Backoffice activates it.
 */
package com.solargrid.app.activities;

import android.os.Bundle;
import android.util.Patterns;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;

import com.solargrid.app.R;
import com.solargrid.app.api.ApiClient;
import com.solargrid.app.models.ApiMessage;
import com.solargrid.app.models.RegisterRequest;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

/**
 * Registration screen for new prosumers. Validates input locally, then
 * POSTs to /api/prosumers/register. New accounts start as Pending.
 */
public class RegisterActivity extends AppCompatActivity {

    private EditText etNic, etName, etEmail, etPhone, etAddress, etPassword;

    /**
     * Sets up the form fields and the Register button click handler.
     */
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_register);

        // Bind views from the layout.
        etNic      = findViewById(R.id.etNic);
        etName     = findViewById(R.id.etName);
        etEmail    = findViewById(R.id.etEmail);
        etPhone    = findViewById(R.id.etPhone);
        etAddress  = findViewById(R.id.etAddress);
        etPassword = findViewById(R.id.etPassword);

        Button btn = findViewById(R.id.btnRegister);
        btn.setOnClickListener(v -> register());
    }

    /**
     * Validates the input on the device, then calls the register endpoint.
     * Client-side validation avoids unnecessary network calls.
     */
    private void register() {
        String nic      = etNic.getText().toString().trim();
        String name     = etName.getText().toString().trim();
        String email    = etEmail.getText().toString().trim();
        String phone    = etPhone.getText().toString().trim();
        String address  = etAddress.getText().toString().trim();
        String password = etPassword.getText().toString();

        // Field-by-field validation (mirrors the DTO rules on the API).
        if (!nic.matches("^([0-9]{9}[vVxX]|[0-9]{12})$")) {
            etNic.setError("Enter a valid NIC");
            return;
        }
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
        if (password.length() < 8) {
            etPassword.setError("At least 8 characters");
            return;
        }

        // Call the API asynchronously. Retrofit delivers the result on the UI thread.
        ApiClient.getService(this)
                .register(new RegisterRequest(nic, name, email, phone, address, password))
                .enqueue(new Callback<ApiMessage>() {

                    /** Handles a successful HTTP response from the server. */
                    @Override
                    public void onResponse(Call<ApiMessage> call, Response<ApiMessage> response) {
                        if (response.isSuccessful()) {
                            Toast.makeText(RegisterActivity.this,
                                    "Registered. Wait for Backoffice to activate your account.",
                                    Toast.LENGTH_LONG).show();
                            finish();
                        } else if (response.code() == 409) {
                            etNic.setError("This NIC is already registered");
                        } else {
                            Toast.makeText(RegisterActivity.this,
                                    "Please check your details and try again.",
                                    Toast.LENGTH_LONG).show();
                        }
                    }

                    /** Handles network errors (no connection, timeout, etc.). */
                    @Override
                    public void onFailure(Call<ApiMessage> call, Throwable t) {
                        Toast.makeText(RegisterActivity.this,
                                "Cannot reach the server.",
                                Toast.LENGTH_LONG).show();
                    }
                });
    }
}