/*
 * File:        ApiClient.java
 * Author:      Shermon H (IT22177964)
 * Description: Creates the Retrofit client used to call the Web API. It adds
 *              the saved JWT token to every request automatically.
 * Created:     29/09/2026
 */

package com.solargrid.app.api;

import android.content.Context;

import com.solargrid.app.BuildConfig;
import com.solargrid.app.db.SessionManager;

import java.util.concurrent.TimeUnit;

import okhttp3.OkHttpClient;
import okhttp3.Request;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;

/**
 * Gives access to the ApiService.
 * Usage: ApiClient.getService(this).login(request).enqueue(...);
 */
public final class ApiClient {

    private static ApiService service;

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
            SessionManager session = new SessionManager(context.getApplicationContext());

            // Adds "Authorization: Bearer <token>" to every request when the user is logged in
            OkHttpClient httpClient = new OkHttpClient.Builder()
                    .addInterceptor(chain -> {
                        Request.Builder builder = chain.request().newBuilder();
                        String token = session.getToken();
                        if (token != null) {
                            builder.header("Authorization", "Bearer " + token);
                        }
                        return chain.proceed(builder.build());
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
}
