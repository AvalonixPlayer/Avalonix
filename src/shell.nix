{ pkgs ? import <nixpkgs> {} }:

let
  xlibs = with pkgs.xorg; [
    libX11
    libICE
    libSM
    libXext
    libXcursor
    libXi
    libXrandr
    libXxf86vm
  ];
  naudiolibs = with pkgs.gst_all_1; [
    gstreamer
    gst-plugins-base
    gst-plugins-good
    gst-plugins-bad
    gst-plugins-ugly
    gst-libav
  ];
in
pkgs.mkShell {
  buildInputs = with pkgs; [
    dotnet-sdk_9
    fontconfig
    libglvnd
    alsa-lib
  ] ++ xlibs ++ naudiolibs ;

  LD_LIBRARY_PATH = pkgs.lib.makeLibraryPath ([
    pkgs.fontconfig
    pkgs.libglvnd
    pkgs.alsa-lib
  ] ++ xlibs);
}
