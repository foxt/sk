//
//  getCurrentTrack.swift
//  sk-mac-applemusic
//
//  Created by foxt on 18/04/2023.
//

import Foundation
import ScriptingBridge

@objc protocol iTunesTrack {
    @objc optional var name: String {get}
    @objc optional var album: String {get}
}

@objc protocol iTunesApplication {
    @objc optional var soundVolume: Int {get}
    @objc optional var currentTrack: iTunesTrack? {get}
    @objc optional var playerPosition: Double {get}
}

extension SBApplication : iTunesApplication {}

var app: iTunesApplication?

func getPlayerPosition(bundleId: String) -> Double? {
    if (app == nil) {
        app = SBApplication(bundleIdentifier: bundleId)
    }
    return app?.playerPosition ?? 0;
}
