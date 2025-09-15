package com.example.devops

import kotlinx.coroutines.runBlocking
import okhttp3.mockwebserver.MockResponse
import okhttp3.mockwebserver.MockWebServer
import org.junit.Assert.assertEquals
import org.junit.Assert.assertNotNull
import org.junit.Test
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory

class ApiServiceTest {
    @Test
    fun `updateScore sends POST with body`() {
        val server = MockWebServer()
        server.enqueue(MockResponse().setResponseCode(200))
        server.start()

        val retrofit = Retrofit.Builder()
            .baseUrl(server.url("/"))
            .addConverterFactory(GsonConverterFactory.create())
            .build()

        val service = retrofit.create(ApiService::class.java)
        runBlocking {
            val user = User(id = 1, name = "Ann", score = 5)
            val response = service.updateScore(user)
            val request = server.takeRequest()
            assertEquals("/api/user/addscore", request.path)
            assertEquals("POST", request.method)
            assertEquals("{\"id\":1,\"name\":\"Ann\",\"score\":5}", request.body.readUtf8())
            assertEquals(200, response.code())
        }

        server.shutdown()
    }

    @Test
    fun `getAllUsers sends GET and parses users`() {
        val server = MockWebServer()
        server.enqueue(MockResponse().setBody("""[{"id":1,"name":"Ann","score":5}]"""))
        server.start()

        val retrofit = Retrofit.Builder()
            .baseUrl(server.url("/"))
            .addConverterFactory(GsonConverterFactory.create())
            .build()

        val service = retrofit.create(ApiService::class.java)
        val response = service.getAllUsers().execute()
        val request = server.takeRequest()
        server.shutdown()

        assertEquals("/api/user/getusers", request.path)
        assertEquals("GET", request.method)
        val users = response.body()
        assertNotNull(users)
        assertEquals(1, users!!.size)
        assertEquals("Ann", users[0].name)
        assertEquals(5, users[0].score)
    }
}
