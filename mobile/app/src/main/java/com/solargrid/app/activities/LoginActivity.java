/*
 * File:        LoginActivity.java
 * Author:      Shermon H (IT22177964)
 * Description: The app's opening screen. Logs the user in through the Web API,
 *              saves the session in SQLite and opens the home screen for the
 *              user's role (Prosumer home or Operator home).
 * Created:     29/09/2026
 */

package com.solargrid.app.activities;

import android.content.Intent;
import android.os.Bundle;
import android.text.TextUtils;
import android.view.View;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.appcompat.app.AppCompatActivity;

import com.google.android.material.button.MaterialButton;
import com.google.android.material.textfield.TextInputEditText;
import com.google.android.material.textfield.TextInputLayout;
import com.solargrid.app.R;
import com.solargrid.app.api.ApiClient;
import com.solargrid.app.db.SessionManager;
import com.solargrid.app.models.LoginRequest;
import com.solargrid.app.models.LoginResponse;
import com.solargrid.app.utils.ApiErrorUtils;
import com.solargrid.app.utils.Constants;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

/**
 * Login screen for prosumers and grid operators.
 */
public class LoginActivity extends AppCompatActivity {

    private TextInputLayout layoutNic;
    private TextInputLayout layoutPassword;
    private TextInputEditText etNic;
    private TextInputEditText etPassword;
    private MaterialButton btnLogin;
    private ProgressBar progressBar;
    private SessionManager session;

    /**
     * Sets up the screen. If the user is already logged in, goes straight to their home screen.
     */
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        session = new SessionManager(this);
        if (session.isLoggedIn()) {
            openHomeForRole(session.getRole());
            return;
        }

        setContentView(R.layout.activity_login);

        layoutNic = findViewById(R.id.layoutNic);
        layoutPassword = findViewById(R.id.layoutPassword);
        etNic = findViewById(R.id.etNic);
        etPassword = findViewById(R.id.etPassword);
        btnLogin = findViewById(R.id.btnLogin);
        progressBar = findViewById(R.id.progressBar);
        TextView tvRegister = findViewById(R.id.tvRegister);

        btnLogin.setOnClickListener(v -> attemptLogin());

        // Member 2: opens the Register screen
        tvRegister.setOnClickListener(v ->
                startActivity(new Intent(this, RegisterActivity.class)));
    }

    /**
     * Checks that both fields are filled in, then sends the login request to the API.
     */
    private void attemptLogin() {
        String nic = etNic.getText() == null ? "" : etNic.getText().toString().trim().toUpperCase();
        String password = etPassword.getText() == null ? "" : etPassword.getText().toString();

        layoutNic.setError(null);
        layoutPassword.setError(null);

        if (TextUtils.isEmpty(nic)) {
            layoutNic.setError(getString(R.string.error_nic_required));
            return;
        }
        if (TextUtils.isEmpty(password)) {
            layoutPassword.setError(getString(R.string.error_password_required));
            return;
        }

        setLoading(true);

        ApiClient.getService(this).login(new LoginRequest(nic, password)).enqueue(new Callback<LoginResponse>() {
            /**
             * Called when the API answers (success or error such as wrong password).
             */
            @Override
            public void onResponse(@NonNull Call<LoginResponse> call, @NonNull Response<LoginResponse> response) {
                setLoading(false);

                if (!response.isSuccessful() || response.body() == null) {
                    Toast.makeText(LoginActivity.this, ApiErrorUtils.getMessage(response), Toast.LENGTH_LONG).show();
                    return;
                }

                LoginResponse login = response.body();

                // Backoffice staff only use the web portal
                if (Constants.ROLE_BACKOFFICE.equals(login.getRole())) {
                    Toast.makeText(LoginActivity.this, R.string.error_backoffice_mobile, Toast.LENGTH_LONG).show();
                    return;
                }

                session.saveSession(login);
                Toast.makeText(LoginActivity.this, "Welcome, " + login.getFullName() + "!", Toast.LENGTH_SHORT).show();
                openHomeForRole(login.getRole());
            }

            /**
             * Called when the API cannot be reached (no network, API not running).
             */
            @Override
            public void onFailure(@NonNull Call<LoginResponse> call, @NonNull Throwable t) {
                setLoading(false);
                Toast.makeText(LoginActivity.this, ApiErrorUtils.getNetworkMessage(t), Toast.LENGTH_LONG).show();
            }
        });
    }

    /**
     * Opens the Operator home for grid operators and the Prosumer home for everyone else,
     * then closes the login screen so Back does not return to it.
     */
    private void openHomeForRole(String role) {
        Class<?> target = Constants.ROLE_GRID_OPERATOR.equals(role)
                ? OperatorHomeActivity.class
                : ProsumerHomeActivity.class;

        startActivity(new Intent(this, target));
        finish();
    }

    /**
     * Shows or hides the loading spinner and disables the button while waiting.
     */
    private void setLoading(boolean loading) {
        progressBar.setVisibility(loading ? View.VISIBLE : View.GONE);
        btnLogin.setEnabled(!loading);
    }
}
