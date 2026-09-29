/*
 * File:        ApiError.java
 * Author:      Shermon H (IT22177964)
 * Description: Shape of every error returned by the Web API: { "message": "..." }
 * Created:     29/09/2026
 */

package com.solargrid.app.models;

/**
 * Error message sent by the API when a request fails.
 */
public class ApiError {

    private String message;

    /**
     * Returns the error message to show to the user.
     */
    public String getMessage() {
        return message;
    }
}
