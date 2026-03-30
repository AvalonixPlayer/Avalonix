import { Metadata } from "./bindings/Metadata";

export async function UpdateTrackUI(metadata: Metadata | null) {
    if (metadata != null && metadata.track_cover != null) {
        const byteArray = new Uint8Array(metadata.track_cover);
        var blob = new Blob([byteArray], {type: "image/jpg"});
        var urlCreator = window.URL || window.webkitURL;
        var imgUrl = urlCreator.createObjectURL(blob);
        var img = document.getElementById('track-cover') as HTMLImageElement;
        img.src = imgUrl;
        setOtherMetadata();

    }
    else if (metadata != null && metadata.track_cover == null)
    {
        var img = document.getElementById('track-cover') as HTMLImageElement;
        const canvas = document.createElement('canvas');
        canvas.width = 512;
        canvas.height = 512;
        const ctx = canvas.getContext('2d');

        ctx!.fillStyle = 'black';
        ctx!.fillRect(0, 0, 512, 512);

        img.src = canvas.toDataURL('image/jpg');
        img.width = 512;
        img.height = 512;
        setOtherMetadata();
    }
    else {
        setOtherMetadata();
    }
    function setOtherMetadata() {
        var title = document.getElementById('track-name') as HTMLTextAreaElement;
        var album = document.getElementById('album-name') as HTMLTextAreaElement;
        title.textContent = metadata?.title || "No title";
        album.textContent = metadata?.album || "No album";
    }
}