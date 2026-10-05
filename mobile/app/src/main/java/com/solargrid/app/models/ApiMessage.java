/*
 * File:        ApiMessage.java
 * Author:      Premaratne R.A.N.C (IT22050908)
 * Description: Simple { message } response returned by the API.
 */
package com.solargrid.app.models;

/**
 * Simple DTO for API responses that only return a message, such as
 * "Registered." or "Profile updated." Mapped from JSON by Gson.
 */
public class ApiMessage {

    /** Human-readable message from the server (e.g. "Profile updated."). */
    public String message;
}
