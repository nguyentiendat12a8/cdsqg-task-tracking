import {formatDateTime} from './formatters';
export function aiInputCode(sequence, receivedAt, isUpload) {
 if(!sequence || !receivedAt)return '';
 const day=formatDateTime(receivedAt).split(' ')[1]?.replaceAll('/','');
 return day?`${isUpload?'TL':'RSS'}-${day}-${String(sequence).padStart(3,'0')}`:'';
}
