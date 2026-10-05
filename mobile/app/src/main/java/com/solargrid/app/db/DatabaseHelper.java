/*
 * File:        DatabaseHelper.java
 * Author:      Shermon H (IT22177964)
 * Description: Creates the local SQLite database on the phone. It stores the
 *              logged-in user (session) and cached reference data (stations).
 *              SQLite is only local storage: the Web API is the source of truth.
 */

package com.solargrid.app.db;

import android.content.Context;
import android.database.sqlite.SQLiteDatabase;
import android.database.sqlite.SQLiteOpenHelper;

/**
 * Opens and creates the SQLite database "solargrid.db".
 * If you add or change a table, increase DATABASE_VERSION so onUpgrade runs.
 */
public class DatabaseHelper extends SQLiteOpenHelper {

    private static final String DATABASE_NAME = "solargrid.db";
    private static final int DATABASE_VERSION = 1;

    // ---------- Session table: the logged-in user (only one row) ----------
    public static final String TABLE_SESSION = "session";
    public static final String COL_NIC = "nic";
    public static final String COL_FULL_NAME = "full_name";
    public static final String COL_ROLE = "role";
    public static final String COL_TOKEN = "token";
    public static final String COL_LOGGED_IN_AT = "logged_in_at";

    // ---------- Stations table: cached list of solar stations (Member 3) ----------
    public static final String TABLE_STATIONS = "stations";
    public static final String COL_STATION_ID = "id";
    public static final String COL_STATION_NAME = "name";
    public static final String COL_STATION_ADDRESS = "address";
    public static final String COL_LATITUDE = "latitude";
    public static final String COL_LONGITUDE = "longitude";
    public static final String COL_CAPACITY_KW = "capacity_kw";
    public static final String COL_BATTERY_SLOTS = "battery_slots";
    public static final String COL_IS_ACTIVE = "is_active";
    public static final String COL_CACHED_AT = "cached_at";

    private static DatabaseHelper instance;

    /**
     * Returns the single shared DatabaseHelper so the whole app uses one connection.
     */
    public static synchronized DatabaseHelper getInstance(Context context) {
        if (instance == null) {
            instance = new DatabaseHelper(context.getApplicationContext());
        }
        return instance;
    }

    /**
     * Private constructor: use getInstance() instead.
     */
    private DatabaseHelper(Context context) {
        super(context, DATABASE_NAME, null, DATABASE_VERSION);
    }

    /**
     * Runs once, the first time the app opens the database. Creates all tables.
     */
    @Override
    public void onCreate(SQLiteDatabase db) {
        db.execSQL("CREATE TABLE " + TABLE_SESSION + " ("
                + COL_NIC + " TEXT PRIMARY KEY, "
                + COL_FULL_NAME + " TEXT, "
                + COL_ROLE + " TEXT, "
                + COL_TOKEN + " TEXT, "
                + COL_LOGGED_IN_AT + " INTEGER)");

        db.execSQL("CREATE TABLE " + TABLE_STATIONS + " ("
                + COL_STATION_ID + " TEXT PRIMARY KEY, "
                + COL_STATION_NAME + " TEXT, "
                + COL_STATION_ADDRESS + " TEXT, "
                + COL_LATITUDE + " REAL, "
                + COL_LONGITUDE + " REAL, "
                + COL_CAPACITY_KW + " REAL, "
                + COL_BATTERY_SLOTS + " INTEGER, "
                + COL_IS_ACTIVE + " INTEGER, "
                + COL_CACHED_AT + " INTEGER)");
    }

    /**
     * Runs when DATABASE_VERSION increases. The local data is only a cache,
     * so the tables are simply recreated.
     */
    @Override
    public void onUpgrade(SQLiteDatabase db, int oldVersion, int newVersion) {
        db.execSQL("DROP TABLE IF EXISTS " + TABLE_SESSION);
        db.execSQL("DROP TABLE IF EXISTS " + TABLE_STATIONS);
        onCreate(db);
    }
}
