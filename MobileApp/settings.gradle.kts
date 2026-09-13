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
include(":feature:playlist")
include(":feature:profile")
include(":feature:playlistsong")
include(":feature:settings")
include(":feature:removesong")
include(":feature:currentmusic")
include(":feature:queue")
include(":core:common")
include(":core:common-ui")
include(":core:model")
include(":core:network")
include(":core:datastore")
include(":core:data")
include(":core:domain")
include(":core:di")
