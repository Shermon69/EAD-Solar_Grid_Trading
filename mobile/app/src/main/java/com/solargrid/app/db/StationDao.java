/*
 * File:        StationDao.java
 * Author:      WMVSB Wahundeniya (IT22292872)
 * Description: Data access object for the "stations" SQLite table. Stations
 *              downloaded from the Web API are saved here so the map can
 *              still show them when the phone is offline.
 */
package com.solargrid.app.db;

import android.content.ContentValues;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;

import com.solargrid.app.models.Station;

import java.util.ArrayList;
import java.util.List;

/**
 * Reads and writes cached solar stations in the local SQLite database.
 */
public class StationDao {
    private final SQLiteDatabase db;

    /**
     * Opens the shared database used by the app.
     *
     * @param dbHelper the app's SQLite helper
     */
    public StationDao(DatabaseHelper dbHelper) {
        this.db = dbHelper.getWritableDatabase();
    }

    /**
     * Replaces the whole cache with the given stations. Used after the full
     * list of active stations has been downloaded from the API.
     *
     * @param stations stations returned by the API
     */
    public void cacheStations(List<Station> stations) {
        db.beginTransaction();
        try {
            db.delete(DatabaseHelper.TABLE_STATIONS, null, null);
            for (Station s : stations) {
                db.insert(DatabaseHelper.TABLE_STATIONS, null, toValues(s));
            }
            db.setTransactionSuccessful();
        } finally {
            db.endTransaction();
        }
    }

    /**
     * Adds or updates the given stations without removing the others. Used
     * for "nearby" results, which are only part of the full list.
     *
     * @param stations stations returned by the API
     */
    public void upsertStations(List<Station> stations) {
        db.beginTransaction();
        try {
            for (Station s : stations) {
                db.insertWithOnConflict(DatabaseHelper.TABLE_STATIONS, null, toValues(s),
                        SQLiteDatabase.CONFLICT_REPLACE);
            }
            db.setTransactionSuccessful();
        } finally {
            db.endTransaction();
        }
    }

    /**
     * Returns all stations saved on the phone, sorted by name.
     *
     * @return cached stations (empty if nothing has been cached yet)
     */
    public List<Station> getCachedStations() {
        List<Station> list = new ArrayList<>();
        Cursor cursor = db.query(DatabaseHelper.TABLE_STATIONS, null, null, null, null, null,
                DatabaseHelper.COL_STATION_NAME + " ASC");
        try {
            while (cursor.moveToNext()) {
                Station s = new Station();
                s.id = cursor.getString(cursor.getColumnIndexOrThrow(DatabaseHelper.COL_STATION_ID));
                s.name = cursor.getString(cursor.getColumnIndexOrThrow(DatabaseHelper.COL_STATION_NAME));
                s.address = cursor.getString(cursor.getColumnIndexOrThrow(DatabaseHelper.COL_STATION_ADDRESS));
                s.latitude = cursor.getDouble(cursor.getColumnIndexOrThrow(DatabaseHelper.COL_LATITUDE));
                s.longitude = cursor.getDouble(cursor.getColumnIndexOrThrow(DatabaseHelper.COL_LONGITUDE));
                s.capacityKw = cursor.getDouble(cursor.getColumnIndexOrThrow(DatabaseHelper.COL_CAPACITY_KW));
                s.batterySlots = cursor.getInt(cursor.getColumnIndexOrThrow(DatabaseHelper.COL_BATTERY_SLOTS));
                s.isActive = cursor.getInt(cursor.getColumnIndexOrThrow(DatabaseHelper.COL_IS_ACTIVE)) == 1;
                list.add(s);
            }
        } finally {
            cursor.close();
        }
        return list;
    }

    /**
     * Converts a station into a database row.
     *
     * @param s the station
     * @return column values for the stations table
     */
    private ContentValues toValues(Station s) {
        ContentValues values = new ContentValues();
        values.put(DatabaseHelper.COL_STATION_ID, s.id);
        values.put(DatabaseHelper.COL_STATION_NAME, s.name);
        values.put(DatabaseHelper.COL_STATION_ADDRESS, s.address);
        values.put(DatabaseHelper.COL_LATITUDE, s.latitude);
        values.put(DatabaseHelper.COL_LONGITUDE, s.longitude);
        values.put(DatabaseHelper.COL_CAPACITY_KW, s.capacityKw);
        values.put(DatabaseHelper.COL_BATTERY_SLOTS, s.batterySlots);
        values.put(DatabaseHelper.COL_IS_ACTIVE, s.isActive ? 1 : 0);
        values.put(DatabaseHelper.COL_CACHED_AT, System.currentTimeMillis());
        return values;
    }
}
