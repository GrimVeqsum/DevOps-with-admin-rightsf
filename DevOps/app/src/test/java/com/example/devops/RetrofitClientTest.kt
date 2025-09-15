package com.example.devops

import okhttp3.mockwebserver.MockResponse
import okhttp3.mockwebserver.MockWebServer
import org.junit.Assert.*
import org.junit.Test
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory

class RetrofitClientTest {
    @Test
    fun `base url constant is correct`() {
        val field = RetrofitClient::class.java.getDeclaredField("BASE_URL")
        field.isAccessible = true
        assertEquals("http://89.169.191.232:7151/", field.get(null))
    }

    @Test
    fun `instance creates api service`() {
        assertNotNull(RetrofitClient.instance)
    }

    @Test
    fun `api service parses users from json`() {
        val server = MockWebServer()
        server.enqueue(MockResponse().setBody("""[{"id":1,"name":"Ann","score":5}]"""))
        server.start()

        val retrofit = Retrofit.Builder()
            .baseUrl(server.url("/"))
            .addConverterFactory(GsonConverterFactory.create())
            .build()

        val service = retrofit.create(ApiService::class.java)
        val response = service.getAllUsers().execute()
        server.shutdown()

        val users = response.body()
        assertNotNull(users)
        assertEquals(1, users!!.size)
        assertEquals("Ann", users[0].name)
        assertEquals(5, users[0].score)
    }
}
