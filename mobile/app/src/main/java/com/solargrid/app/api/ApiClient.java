/*
 * File:        ApiClient.java
 * Author:      Shermon H (IT22177964)
 * Description: Creates the Retrofit client used to call the Web API. It adds
 *              the saved JWT token to every request automatically.
 */

package com.solargrid.app.api;

import android.content.Context;
import android.content.Intent;
import android.os.Handler;
import android.os.Looper;

import com.solargrid.app.BuildConfig;
import com.solargrid.app.activities.LoginActivity;
import com.solargrid.app.db.SessionManager;
import com.solargrid.app.utils.Constants;

import java.util.concurrent.TimeUnit;

import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.Response;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;

/**
 * Gives access to the ApiService.
 * Usage: ApiClient.getService(this).login(request).enqueue(...);
 */
public final class ApiClient {

    private static ApiService service;

    // Stops several failing calls from opening the login screen many times in a row
    private static final Object REDIRECT_LOCK = new Object();
    private static long lastRedirectAt = 0;

    /**
     * Private constructor: use getService() instead.
     */
    private ApiClient() {
    }

    /**
     * Returns the shared ApiService, creating it the first time.
     */
    public static synchronized ApiService getService(Context context) {
        if (service == null) {
            Context appContext = context.getApplicationContext();
            SessionManager session = new SessionManager(appContext);

            // Adds "Authorization: Bearer <token>" to every request when the user is logged in,
            // and sends the user back to login when the API says the token is no longer valid
            OkHttpClient httpClient = new OkHttpClient.Builder()
                    .addInterceptor(chain -> {
                        Request request = chain.request();
                        Request.Builder builder = request.newBuilder();
                        String token = session.getToken();
                        if (token != null) {
                            builder.header("Authorization", "Bearer " + token);
                        }

                        Response response = chain.proceed(builder.build());

                        // 401 on a logged-in request = expired or invalid token.
                        // A wrong password on the login call also gives 401, so that call is skipped.
                        if (response.code() == 401 && token != null && !isLoginRequest(request)) {
                            handleExpiredSession(appContext, session);
                        }
                        return response;
                    })
                    .connectTimeout(15, TimeUnit.SECONDS)
                    .readTimeout(20, TimeUnit.SECONDS)
                    .build();

            Retrofit retrofit = new Retrofit.Builder()
                    .baseUrl(BuildConfig.API_BASE_URL)
                    .client(httpClient)
                    .addConverterFactory(GsonConverterFactory.create())
                    .build();

            service = retrofit.create(ApiService.class);
        }
        return service;
    }

    /**
     * Returns true if the request is the login call (POST auth/login).
     */
    private static boolean isLoginRequest(Request request) {
        return request.url().encodedPath().endsWith("/auth/login");
    }

    /**
     * Clears the saved session and opens the login screen with a "session expired" message.
     * The screen stack is cleared so the Back button cannot return to a screen that needs login.
     */
    private static void handleExpiredSession(Context appContext, SessionManager session) {
        synchronized (REDIRECT_LOCK) {
            long now = System.currentTimeMillis();
            if (now - lastRedirectAt < 3000) {
                return;
            }
            lastRedirectAt = now;
        }

        session.logout();

        // Screens can only be opened from the main thread, but this runs on a network thread
        new Handler(Looper.getMainLooper()).post(() -> {
            Intent intent = new Intent(appContext, LoginActivity.class);
            intent.putExtra(Constants.EXTRA_SESSION_EXPIRED, true);
            intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
            appContext.startActivity(intent);
        });
    }
}
