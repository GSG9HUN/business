rootProject.name = "MobileApp"

pluginManagement {
    repositories {
        google {
            mavenContent {
                includeGroupAndSubgroups("androidx")
                includeGroupAndSubgroups("com.android")
                includeGroupAndSubgroups("com.google")
            }
        }
        mavenCentral()
        gradlePluginPortal()
    }
}

dependencyResolutionManagement {
    repositories {
        google {
            mavenContent {
                includeGroupAndSubgroups("androidx")
                includeGroupAndSubgroups("com.android")
                includeGroupAndSubgroups("com.google")
            }
        }
        mavenCentral()
    }
}

include(":androidApp")
include(":shared")
include(":feature:login")
include(":feature:guild")
include(":feature:playlists")
include(":feature:profile")
include(":feature:addsong")
include(":feature:settings")
include(":feature:removesong")
include(":feature:currentmusic")
include(":feature:queue")
include(":core:common")
include(":core:ui")
include(":core:network")
include(":core:auth")
include(":core:settings")
