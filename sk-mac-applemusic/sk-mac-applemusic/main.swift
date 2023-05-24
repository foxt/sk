//
//  main.swift
//  sk-mac-applemusic
//
//  Created by foxt on 18/04/2023.
//

import Foundation
import Cocoa
import Darwin.C;
setbuf(__stdoutp, nil);

let dnc = DistributedNotificationCenter.default()


var isPlaying = false
// We need to know which bundle ID to request data from (com.apple.iTunes or
// com.apple.Music).
// On older systems with iTunes, we recieve only the
// com.apple.iTunes.playerInfo event, and thus need to send AppleEvents to
// iTunes bundle ID.
// However, on newer systems, *both* com.apple.iTunes.playerInfo &
// com.apple.Music.playerInfo are recieved, but we should filter out the
// duplicate iTunes events, and note down that we should send events to the
// Music bundle ID (sending to iTunes bundle ID will fail on new systems).
// We can get away with having a boolean here, as we will only ever send events
// when isPlaying is true, meaning we have already recieved a Notification. The
// first notification will be duplicated as AM sends the legacy iTunes event
// first, but this isn't too big a deal.
var isAppleMusicApp = false
var lastId = ""

func notificationRecieved(notif: Notification) {
    if (notif.name.rawValue == "com.apple.Music.playerInfo") { isAppleMusicApp = true }
    else if (notif.name.rawValue == "com.apple.iTunes.playerInfo" && isAppleMusicApp) { return }
    
    
    let data = notif.userInfo!
    let state = (data["Player State"] as! String)
    
    // 0 - playing
    // 1 - paused
    // 2 - stopped
    var numericState = 0
    isPlaying = false

    switch (state) {
        case "Playing":
            numericState = 0
            isPlaying = true
            break
        case "Paused":
            numericState = 1
            break
        default:
            numericState = 2
            break
    }
    print("s" + String(numericState))
    

    // If the media is loading we don't know the time. Time is required for
    // scrobbling so we just skip this notification and hope we recieve one
    // when playback actually starts. We send the media stopped event in
    // this case i.e. for radio
    if (data["Total Time"] == nil) {
        print("s2")
        isPlaying = false
        return
    }
    var id = data["Store URL"]
    if (id == nil) { id = data["PersistentID"] }
    if (id == nil) { id = data["Name"] }
    if (id as! String == lastId) { return }
    lastId = id as! String
    printJson(prefix: "t", obj: [
        "id": id ?? "what",
        "title": data["Name"] ?? "",
        "artist": data["Artist"] ?? "",
        "album": data["Album"] ?? "",
        "albumArtist": data["Album Artist"] ?? "",
        "duration": (data["Total Time"] as! Int) / 1000,
    ])

}

dnc.addObserver(forName: NSNotification.Name("com.apple.Music.playerInfo"), object: nil,queue: nil) { notification in notificationRecieved(notif: notification) }
dnc.addObserver(forName: NSNotification.Name("com.apple.iTunes.playerInfo"), object: nil,queue: nil) { notification in notificationRecieved(notif: notification)  }


// Report the current track progress. we do this instead of just assuming the
// current position because there are many edgecases where the player can stop
// without submitting a pause/stop event.
// (i.e. it crashes, it's force quat(???), it's buffering, etc etc)
Timer.scheduledTimer(withTimeInterval: 3,  repeats: true) { timer in
    // only report if we know the player is playing for performance & because
    // AppleEvents will open the app if it's not already open.
    
    if (!isPlaying) {return}
    let bundleId = isAppleMusicApp ? "com.apple.Music" : "com.apple.iTunes"
    print("p" + String(getPlayerPosition(bundleId: bundleId) ?? 0))
}

RunLoop.main.run()
