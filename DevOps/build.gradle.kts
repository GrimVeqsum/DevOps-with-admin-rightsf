// Top-level build file where you can add configuration options common to all sub-projects/modules.
plugins {
    id("com.android.application") version "8.2.2" apply false
    id("org.jetbrains.kotlin.android") version "1.9.22" apply false
    id("org.sonarqube") version "3.5.0.2730"
}

sonar {
    properties {
        property("sonar.projectKey", "DevOps2")
        property("sonar.host.url", System.getenv("SONAR_HOST_URL") ?: "http://89.169.191.232:9000")
    }
}
