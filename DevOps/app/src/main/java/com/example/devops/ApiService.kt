package com.example.devops

import retrofit2.Call
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.POST
import retrofit2.http.Path

interface ApiService {
    @POST("api/user/addscore")
    suspend fun updateScore(@Body user: User): Response<Void>

    @GET("api/user/getusers")
    fun getAllUsers(): Call<List<User>>
}
//https://10.0.2.2:7151/api/user/getusers
//http://10.0.2.2:5198/api/user/getusers