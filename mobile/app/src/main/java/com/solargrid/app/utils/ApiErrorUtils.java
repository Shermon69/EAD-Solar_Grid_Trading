/*
 * File:        ApiErrorUtils.java
 * Author:      Shermon H (IT22177964)
 * Description: Turns failed API calls into short messages that can be shown
 *              to the user (e.g. in a Toast).
 * Created:     29/09/2026
 */

package com.solargrid.app.utils;

import com.google.gson.Gson;
import com.solargrid.app.models.ApiError;

import java.io.IOException;

import retrofit2.Response;

/**
 * Helper methods for reading API error messages.
 */
public final class ApiErrorUtils {

    /**
     * Private constructor: this class only has static methods.
     */
    private ApiErrorUtils() {
    }

    /**
     * Reads the { "message": "..." } body of a failed response.
     * Use in onResponse when response.isSuccessful() is false.
     */
    public static String getMessage(Response<?> response) {
        try {
            if (response.errorBody() != null) {
                ApiError error = new Gson().fromJson(response.errorBody().string(), ApiError.class);
                if (error != null && error.getMessage() != null) {
                    return error.getMessage();
                }
            }
        } catch (IOException | RuntimeException ignored) {
            // Fall through to the general message below
        }

        if (response.code() == 401) {
            return "Your session has expired. Please log in again.";
        }
        return "Something went wrong (" + response.code() + "). Please try again.";
    }

    /**
     * Message for when the API cannot be reached at all.
     * Use in onFailure.
     */
    public static String getNetworkMessage(Throwable t) {
        return "Cannot connect to the server. Check your internet connection and that the API is running.";
    }
}
