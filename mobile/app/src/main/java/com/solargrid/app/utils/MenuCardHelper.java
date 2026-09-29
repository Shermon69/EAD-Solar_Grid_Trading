/*
 * File:        MenuCardHelper.java
 * Author:      Shermon H (IT22177964)
 * Description: Fills in a menu card (item_menu_card.xml) on the home screens
 *              with an icon, title, description and click action.
 * Created:     29/09/2026
 */

package com.solargrid.app.utils;

import android.content.res.ColorStateList;
import android.view.View;
import android.widget.ImageView;
import android.widget.TextView;

import androidx.core.content.ContextCompat;

import com.solargrid.app.R;

/**
 * Helper for setting up home screen menu cards.
 */
public final class MenuCardHelper {

    /**
     * Private constructor: this class only has static methods.
     */
    private MenuCardHelper() {
    }

    /**
     * Sets the icon, texts and click action of one menu card.
     *
     * @param card      the included card view
     * @param iconRes   drawable for the icon
     * @param titleRes  string for the title
     * @param descRes   string for the short description
     * @param colorRes  colour of the icon
     * @param onClick   what happens when the card is tapped
     */
    public static void setup(View card, int iconRes, int titleRes, int descRes, int colorRes,
                             View.OnClickListener onClick) {
        ImageView icon = card.findViewById(R.id.cardIcon);
        TextView title = card.findViewById(R.id.cardTitle);
        TextView subtitle = card.findViewById(R.id.cardSubtitle);

        icon.setImageResource(iconRes);
        icon.setImageTintList(ColorStateList.valueOf(ContextCompat.getColor(card.getContext(), colorRes)));
        title.setText(titleRes);
        subtitle.setText(descRes);
        card.setOnClickListener(onClick);
    }
}
