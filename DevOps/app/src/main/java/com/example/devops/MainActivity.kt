package com.example.devops

import androidx.appcompat.app.AppCompatActivity
import android.os.Bundle
import android.webkit.JavascriptInterface
import android.webkit.WebView
import android.widget.EditText
import android.widget.TableLayout
import android.widget.TableRow
import android.widget.TextView
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

open class MainActivity : AppCompatActivity() {
    lateinit var webView: WebView
    lateinit var editText: EditText
    private val url = "file:///android_asset/index.html"
    internal lateinit var apiService: ApiService
    internal lateinit var tableLayout: TableLayout

    internal var users: List<User> = emptyList()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)
        webView = findViewById(R.id.wv)
        editText = findViewById(R.id.editText)
        editText.setText("user")

        apiService = RetrofitClient.instance
        webView.addJavascriptInterface(WebAppInterface(), "Android")
        webView.settings.javaScriptEnabled = true
        webView.loadUrl(url)

        tableLayout = findViewById(R.id.tableLayout)

        fetchUsers()
    }

    inner class WebAppInterface {

        @JavascriptInterface
        fun onGameOver(distanceMeter: Int) {
            val inputText = editText.text.toString()
            val score = distanceMeter.toFloat().toInt()
            CoroutineScope(Dispatchers.IO).launch {
                updateUserScore(User(null, inputText, score))
            }
        }
    }

    fun fillTable(allUsers: List<User>) {
        runOnUiThread {
            tableLayout.removeAllViews()

            for (user in allUsers) {
                val tableRow = TableRow(this).apply {
                    layoutParams = TableLayout.LayoutParams(
                        TableLayout.LayoutParams.MATCH_PARENT,
                        TableLayout.LayoutParams.WRAP_CONTENT
                    )
                }

                val nameTextView = TextView(this).apply {
                    text = user.name
                    setPadding(8, 8, 8, 8)
                }
                tableRow.addView(nameTextView)

                val scoreTextView = TextView(this).apply {
                    text = user.score.toString()
                    setPadding(8, 8, 8, 8)
                }
                tableRow.addView(scoreTextView)

                tableLayout.addView(tableRow)
            }
            tableLayout.requestLayout()
        }
    }

    fun fetchUsers() {
        val call: Call<List<User>> = apiService.getAllUsers()

        call.enqueue(object : Callback<List<User>> {
            override fun onResponse(call: Call<List<User>>, response: Response<List<User>>) {
                if (response.isSuccessful) {
                    response.body()?.let {
                        users = it.sortedByDescending { user -> user.score }
                        fillTable(users)
                    }
                } else {
                    println("Error: ${response.errorBody()}")
                }
            }

            override fun onFailure(call: Call<List<User>>, t: Throwable) {
                println("Exception: ${t.message}")
            }
        })
    }

    suspend fun updateUserScore(user: User) {
        try {
            val response = apiService.updateScore(user)

            if (response.isSuccessful) {
                runOnUiThread {
                    println("Score updated!")
                    fetchUsers()
                }
            } else {
                runOnUiThread {
                    println("Error: ${response.errorBody()}")
                }
            }
        } catch (e: Exception) {
            runOnUiThread {
                println("Exception: ${e.message}")
            }
        }
    }
}
