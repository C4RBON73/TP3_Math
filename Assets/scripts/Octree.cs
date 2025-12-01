using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.iOS.Xcode;
using UnityEngine;


public class Octree : MonoBehaviour
{

    const int NB_ENFANTS = 8;
    [SerializeField] Vector3 tailleMax;
    [SerializeField] int maxProfondeur;
    Nodes racine;
    Bounds delimitation;

    [SerializeField] sphereOctree[] l_sphere;




    public class sphereOctree
    {
        public float rayon;
        public Vector3 center;

        public sphereOctree(float ray, Vector3 cen)
        {
            this.rayon = ray;
            this.center = cen;
        }
    }

    public class Nodes
    {
        const int NB_ENFANTS = 8;


        public int profondeur;
        public int plein;
        public Bounds delimitation;
        public Nodes[] enfants;
        public bool voxel()
        {
            return enfants == null || profondeur == 0 ;
        }


        public Nodes(int prof, Bounds delim, int plei)
        {
            //Debug.Log("Création d'un Nodes à la profondeur " + prof + " de type " + plei + " centré en " + delim.center + " et de taille " + delim.size);
            delimitation = delim;
            profondeur = prof;

            //Si la profondeur est différente de 0, on génère des enfants
            if (profondeur != 0)
            {
                enfants = new Nodes[NB_ENFANTS];
                
                for (int i = 0; i < NB_ENFANTS; i++)
                {
                    Bounds b = delimitation;
                    b.size /= 2f ;
                    b.center += new Vector3(
                        ((i & 1) == 0) ? -b.size.x / 2f : b.size.x / 2f,
                        ((i & 2) == 0) ? -b.size.y / 2f : b.size.y / 2f,
                        ((i & 4) == 0) ? -b.size.z / 2f : b.size.z / 2f
                        );
                    enfants[i] = new Nodes(profondeur -1,b,plei);
                }
            }
            //Sinon c'est un voxel, on lui assigne une valeur
            else
            {
                plein = plei;

            }
        }

    }



    private void renderOctree(Nodes current)
    {
        if (!current.voxel())
        {
            for (int i = 0;i < NB_ENFANTS; i++)
            {
                renderOctree(current.enfants[i]);
            }
        }
        else
        {
            if (current.plein == 1)
            {
                GameObject voxelCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                voxelCube.transform.position = current.delimitation.center;
                voxelCube.transform.localScale = current.delimitation.size;
            }
        }

    }

    private void buildSphere( Nodes node, sphereOctree[] sp)
    {
        voxelInSphere(node,sp);
        if (!node.voxel())
        {
            for (int i = 0; i < NB_ENFANTS; i++)
            {
                buildSphere(node.enfants[i], sp);
            }
        }
        


    }

    private void voxelInSphere(Nodes voxel, sphereOctree[] sp)
    {
        float x, y, z;

        int nbSphereBounds = 0;
        for (int j = 0; j < sp.Length; j++)
        {
            int inBounds = 0;
            for (int i = 0; i < NB_ENFANTS; i++)
            {

                x = ((i == 1) || (i == 2) || (i == 5) || (i == 6)) ? voxel.delimitation.center.x + voxel.delimitation.size.x : voxel.delimitation.center.x;
                y = (i < 4) ? voxel.delimitation.center.y + voxel.delimitation.size.y : voxel.delimitation.center.y;
                z = ((i % 4) - 2 >= 0) ? voxel.delimitation.center.z + voxel.delimitation.size.z : voxel.delimitation.center.z;


                float pos = Mathf.Pow(x - sp[j].center.x, 2) + Mathf.Pow(y - sp[j].center.y, 2) + Mathf.Pow(z - sp[j].center.z, 2);
                if (pos < Mathf.Pow(sp[j].rayon, 2))
                {
                    inBounds++;
                }
            }



            //Vecteur entre sphere et cube
            Vector3 vect = voxel.delimitation.center - sp[j].center;

            //ppp = point le plus proche
            Vector3 ppp = vect.normalized * sp[j].rayon;





            if (inBounds == 8)
            {
                nbSphereBounds++;
            }

            if (voxel.delimitation.ClosestPoint(ppp) == ppp)
            {
                //TODO :Si a profondeurMax on crée des enfants
                
            }

        }
        if (nbSphereBounds > 0)
        {
            voxel.plein = 1;
            voxel.enfants = null;
            voxel.profondeur = 0;
        }
        else
        {
            if( voxel.profondeur ==0)
            {
                voxel.plein = 0;
                voxel.enfants = null;
            }
        }

    }

    void initSphere()
    {
        l_sphere = new sphereOctree[2];
        sphereOctree s1 = new sphereOctree(10, new Vector3(0,0,0));
        sphereOctree s2 = new sphereOctree(10, new Vector3(20, 0, 0));
        //sphereOctree s3 = new sphereOctree(10, new Vector3(20, 0, 20));
        l_sphere[0] = s1;
        l_sphere[1] = s2;
        //l_sphere[2] = s3;

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initSphere();

        delimitation = new Bounds(transform.position,tailleMax );
        racine = new Nodes(maxProfondeur, delimitation, 1);
        

        buildSphere(racine, l_sphere);


        
        renderOctree(racine);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
