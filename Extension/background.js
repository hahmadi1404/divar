// Copyright 2018 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

'use strict';

chrome.runtime.onMessage.addListener(function(message,sender,sendResponse){
    if(message.pressEnter){
        chrome.tabs.query({active: true}, function(tabs) {
            chrome.debugger.attach({ tabId: tabs[0].id }, "1.0");
            chrome.debugger.sendCommand({ tabId: tabs[0].id }, 'Input.dispatchKeyEvent', { type: 'keyUp', windowsVirtualKeyCode:65, nativeVirtualKeyCode : 65, macCharCode: 65  });
            chrome.debugger.sendCommand({ tabId: tabs[0].id }, 'Input.dispatchKeyEvent', { type: 'keyDown', windowsVirtualKeyCode:65, nativeVirtualKeyCode : 65, macCharCode: 65  });
            chrome.debugger.detach({ tabId: tabs[0].id });
        });
    }
});
