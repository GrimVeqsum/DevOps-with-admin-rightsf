package com.example.devops

import retrofit2.Call
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.POST

interface ApiService {
    @POST("api/user/addscore")
    suspend fun updateScore(@Body user: User): Response<Void>

    @GET("api/user/getusers")
    fun getAllUsers(): Call<List<User>>
}
