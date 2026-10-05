/*
 * File:        SessionManager.java
 * Author:      Shermon H (IT22177964)
 * Description: Saves, reads and clears the logged-in user in the SQLite
 *              "session" table, so the user stays logged in after closing the app.
 */

package com.solargrid.app.db;

import android.content.ContentValues;
import android.content.Context;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.util.Base64;

import com.solargrid.app.models.LoginResponse;

import org.json.JSONException;
import org.json.JSONObject;

import java.nio.charset.StandardCharsets;

/**
 * Stores the current login session in SQLite.
 * Usage: SessionManager session = new SessionManager(this); session.getNic();
 */
public class SessionManager {

    private final DatabaseHelper dbHelper;

    /**
     * Creates a SessionManager that uses the app's SQLite database.
     */
    public SessionManager(Context context) {
        this.dbHelper = DatabaseHelper.getInstance(context);
    }

    /**
     * Saves the logged-in user. Any previous session is removed first.
     */
    public void saveSession(LoginResponse login) {
        SQLiteDatabase db = dbHelper.getWritableDatabase();
        db.delete(DatabaseHelper.TABLE_SESSION, null, null);

        ContentValues values = new ContentValues();
        values.put(DatabaseHelper.COL_NIC, login.getNic());
        values.put(DatabaseHelper.COL_FULL_NAME, login.getFullName());
        values.put(DatabaseHelper.COL_ROLE, login.getRole());
        values.put(DatabaseHelper.COL_TOKEN, login.getToken());
        values.put(DatabaseHelper.COL_LOGGED_IN_AT, System.currentTimeMillis());
        db.insert(DatabaseHelper.TABLE_SESSION, null, values);
    }

    /**
     * Returns true if a user is logged in with a token that has not run out yet.
     */
    public boolean isLoggedIn() {
        return getToken() != null && !isSessionExpired();
    }

    /**
     * Returns true if a token is saved but its expiry time (the "exp" claim inside the
     * JWT) has already passed. A token that cannot be read is treated as not expired,
     * because the API will answer 401 if it is really invalid.
     */
    public boolean isSessionExpired() {
        String token = getToken();
        if (token == null) {
            return false;
        }

        try {
            // A JWT is header.payload.signature; the payload is Base64-URL encoded JSON
            String[] parts = token.split("\\.");
            if (parts.length < 2) {
                return false;
            }

            byte[] payload = Base64.decode(parts[1], Base64.URL_SAFE | Base64.NO_PADDING | Base64.NO_WRAP);
            long expirySeconds = new JSONObject(new String(payload, StandardCharsets.UTF_8)).optLong("exp", 0);

            return expirySeconds > 0 && expirySeconds * 1000L <= System.currentTimeMillis();
        } catch (IllegalArgumentException | JSONException e) {
            return false;
        }
    }

    /**
     * Returns the saved JWT token, or null if nobody is logged in.
     */
    public String getToken() {
        return readColumn(DatabaseHelper.COL_TOKEN);
    }

    /**
     * Returns the logged-in user's NIC.
     */
    public String getNic() {
        return readColumn(DatabaseHelper.COL_NIC);
    }

    /**
     * Returns the logged-in user's full name.
     */
    public String getFullName() {
        return readColumn(DatabaseHelper.COL_FULL_NAME);
    }

    /**
     * Returns the logged-in user's role (Prosumer or GridOperator).
     */
    public String getRole() {
        return readColumn(DatabaseHelper.COL_ROLE);
    }

    /**
     * Updates the saved name, e.g. after the user edits their profile.
     */
    public void updateFullName(String fullName) {
        ContentValues values = new ContentValues();
        values.put(DatabaseHelper.COL_FULL_NAME, fullName);
        dbHelper.getWritableDatabase().update(DatabaseHelper.TABLE_SESSION, values, null, null);
    }

    /**
     * Logs the user out by deleting the session row.
     */
    public void logout() {
        dbHelper.getWritableDatabase().delete(DatabaseHelper.TABLE_SESSION, null, null);
    }

    /**
     * Reads one column from the session row. Returns null if there is no session.
     */
    private String readColumn(String column) {
        SQLiteDatabase db = dbHelper.getReadableDatabase();
        try (Cursor cursor = db.query(DatabaseHelper.TABLE_SESSION, new String[]{column},
                null, null, null, null, null, "1")) {
            if (cursor.moveToFirst()) {
                return cursor.getString(0);
            }
            return null;
        }
    }
}
