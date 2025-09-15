package com.example.devops

import android.os.Looper
import android.widget.TableRow
import android.widget.TextView
import org.junit.Assert.assertEquals
import org.junit.Assert.assertNotNull
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.Robolectric
import org.robolectric.RobolectricTestRunner
import org.robolectric.Shadows

@RunWith(RobolectricTestRunner::class)
class MainActivityTest {

    @Test
    fun `onCreate loads webView with url`() {
        val activity = Robolectric.buildActivity(MainActivity::class.java).setup().get()
        assertNotNull(activity.webView)
        assertEquals("file:///android_asset/index.html", activity.webView.url)
    }

    @Test
    fun `fillTable adds rows for users`() {
        val activity = Robolectric.buildActivity(MainActivity::class.java).setup().get()
        val users = listOf(User(1, "Alice", 5), User(2, "Bob", 7))
        activity.fillTable(users)
        Shadows.shadowOf(Looper.getMainLooper()).idle()
        assertEquals(users.size, activity.tableLayout.childCount)
        val firstRow = activity.tableLayout.getChildAt(0) as TableRow
        val nameTv = firstRow.getChildAt(0) as TextView
        val scoreTv = firstRow.getChildAt(1) as TextView
        assertEquals("Alice", nameTv.text)
        assertEquals("5", scoreTv.text.toString())
    }
}
