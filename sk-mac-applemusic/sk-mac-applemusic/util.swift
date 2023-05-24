//
//  util.swift
//  sk-mac-applemusic
//
//  Created by foxt on 18/04/2023.
//

import Foundation


func printJson(prefix: String, obj: Any) {
    do {
        print(prefix + (String(data: (try (JSONSerialization.data(withJSONObject: obj))), encoding: .utf8) ?? "{}"))
    } catch{}
}
