package com.example.devops
import com.google.gson.annotations.SerializedName

data class User(
    @SerializedName("id") val id: Int? = null,
    @SerializedName("name") val name: String,
    @SerializedName("score") val score: Int
    // Добавьте другие поля, если они есть на сервере
)