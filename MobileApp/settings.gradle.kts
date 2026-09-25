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
include(":feature:login:presentation")
include(":feature:login:ui")
include(":feature:guild:presentation")
include(":feature:guild:ui")
include(":feature:playlist:presentation")
include(":feature:playlist:ui")
include(":feature:profile:presentation")
include(":feature:profile:ui")
include(":feature:playlistsong:presentation")
include(":feature:playlistsong:ui")
include(":feature:settings:presentation")
include(":feature:settings:ui")
include(":feature:removesong:presentation")
include(":feature:removesong:ui")
include(":feature:currenttrack:presentation")
include(":feature:currenttrack:ui")
include(":feature:queue:presentation")
include(":feature:queue:ui")
include(":core:common")
include(":core:common-ui")
include(":core:model")
include(":core:network")
include(":core:datastore")
include(":core:data")
include(":core:domain")
include(":core:di")

