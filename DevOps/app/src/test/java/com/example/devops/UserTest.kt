package com.example.devops

import org.junit.Assert.assertEquals
import org.junit.Test


import com.google.gson.Gson
import org.junit.Assert.*

class UserTest {

    @Test
    fun `test User data class properties`() {
        // Arrange
        val id = 1
        val name = "Test User"
        val score = 100

        // Act
        val user = User(id, name, score)

        // Assert
        assertEquals(id, user.id)
        assertEquals(name, user.name)
        assertEquals(score, user.score)
    }

    @Test
    fun `test User with null id`() {
        // Arrange
        val name = "Test User"
        val score = 100

        // Act
        val user = User(name = name, score = score)

        // Assert
        assertNull(user.id)
        assertEquals(name, user.name)
        assertEquals(score, user.score)
    }

    @Test
    fun `test User equality`() {
        // Arrange
        val user1 = User(1, "Alice", 85)
        val user2 = User(1, "Alice", 85)

        // Act & Assert
        assertEquals(user1, user2)
        assertEquals(user1.hashCode(), user2.hashCode())
    }

    @Test
    fun `test User inequality`() {
        // Arrange
        val user1 = User(1, "Alice", 85)
        val user2 = User(2, "Bob", 90)

        // Act & Assert
        assertNotEquals(user1, user2)
    }

    @Test
    fun `test User toString`() {
        // Arrange
        val user = User(1, "Alice", 85)
        val expectedString = "User(id=1, name=Alice, score=85)"

        // Act & Assert
        assertEquals(expectedString, user.toString())
    }

    @Test
    fun `test User JSON serialization`() {
        // Arrange
        val user = User(1, "Alice", 85)
        val gson = Gson()
        val expectedJson = """{"id":1,"name":"Alice","score":85}"""

        // Act
        val json = gson.toJson(user)

        // Assert
        assertEquals(expectedJson, json)
    }

    @Test
    fun `test User JSON deserialization`() {
        // Arrange
        val json = """{"id":1,"name":"Alice","score":85}"""
        val gson = Gson()
        val expectedUser = User(1, "Alice", 85)

        // Act
        val user = gson.fromJson(json, User::class.java)

        // Assert
        assertEquals(expectedUser, user)
    }

    @Test
    fun `test User JSON deserialization with null id`() {
        // Arrange
        val json = """{"name":"Alice","score":85}"""
        val gson = Gson()
        val expectedUser = User(name = "Alice", score = 85)

        // Act
        val user = gson.fromJson(json, User::class.java)

        // Assert
        assertEquals(expectedUser, user)
        assertNull(user.id)
    }
}