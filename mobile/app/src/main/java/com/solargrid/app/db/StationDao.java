/*
 * File:        StationDao.java
 * Author:      WMVSB Wahundeniya (IT22292872)
 * Description: DAO for caching stations in SQLite.
 */
package com.solargrid.app.db;

import android.content.ContentValues;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;

import com.solargrid.app.models.Station;

import java.util.ArrayList;
import java.util.List;

public class StationDao {
    private final SQLiteDatabase db;

    /**
     * Initializes a new instance of StationDao.
     */
    public StationDao(DatabaseHelper dbHelper) {
        this.db = dbHelper.getWritableDatabase();
    }

    /**
     * Executes the cacheStations operation.
     */
    public void cacheStations(List<Station> stations) {
        db.beginTransaction();
        try {
            db.delete(DatabaseHelper.TABLE_STATIONS, null, null);
            for (Station s : stations) {
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
                db.insert(DatabaseHelper.TABLE_STATIONS, null, values);
            }
            db.setTransactionSuccessful();
        } finally {
            db.endTransaction();
        }
    }

    /**
     * Executes the getCachedStations operation.
     */
    public List<Station> getCachedStations() {
        List<Station> list = new ArrayList<>();
        Cursor cursor = db.query(DatabaseHelper.TABLE_STATIONS, null, null, null, null, null, null);
        if (cursor.moveToFirst()) {
            do {
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
            } while (cursor.moveToNext());
        }
        cursor.close();
        return list;
    }
}
