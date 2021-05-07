import { eventManager } from "./global"
import { getObjectId } from "./env"
import { Object } from "./object"
import { log as importedLog, } from "./env"

const OBJECT_NOT_FOUND_ID = -1

export function getObjectByName(name: string): Object | null {
    for (let i = 0; i < name.length; i++) {
        store<u8>(i, name.charCodeAt(i))
    }
    const id = getObjectId(name.length)
    if (id === OBJECT_NOT_FOUND_ID) {
      return null
    } 
    return new Object(id, eventManager)
}

export function log(type: i32): void {
  importedLog(type)
}