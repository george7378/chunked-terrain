xof 0303txt 0032

Frame Root {
  FrameTransformMatrix {
     1.000000, 0.000000, 0.000000, 0.000000,
     0.000000,-0.000000,-1.000000, 0.000000,
     0.000000, 1.000000,-0.000000, 0.000000,
     0.000000, 0.000000, 0.000000, 1.000000;;
  }
  Frame Plane {
    FrameTransformMatrix {
       1.000000, 0.000000, 0.000000, 0.000000,
       0.000000, 1.000000, 0.000000, 0.000000,
       0.000000, 0.000000, 1.000000, 0.000000,
       0.000000, 0.000000, 0.000000, 1.000000;;
    }
    Mesh { // Plane mesh
      4;
      -0.500000;-0.000000; 0.500000;,
       0.500000;-0.000000; 0.500000;,
       0.500000; 0.000000;-0.500000;,
      -0.500000; 0.000000;-0.500000;;
      1;
      4;0,1,2,3;;
      MeshNormals { // Plane normals
        1;
         0.000000; 1.000000; 0.000000;;
        1;
        4;0,0,0,0;;
      } // End of Plane normals
      MeshTextureCoords { // Plane UV coordinates
        4;
         0.000000; 1.000000;,
         1.000000; 1.000000;,
         1.000000; 0.000000;,
         0.000000; 0.000000;;
      } // End of Plane UV coordinates
    } // End of Plane mesh
  } // End of Plane
} // End of Root
