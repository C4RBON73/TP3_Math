using Unity.VisualScripting;
using UnityEngine;


public class Octree : MonoBehaviour
{
    const int NB_ENFANTS = 8;
    [SerializeField] Vector3 tailleMax;
    [SerializeField] int maxProfondeur;
    Nodes racine;
    Bounds delimitation;
    

    public class Nodes
    {
        const int NB_ENFANTS = 8;


        int profondeur;
        public int plein;
        public Bounds delimitation;
        public Nodes[] enfants;
        public bool voxel()
        {
            return enfants == null || profondeur == 0;
        }


        public Nodes(int prof, Bounds delim, int plei)
        {
            Debug.Log("Création d'un Nodes à la profondeur " + prof + " de type " + plei + " centré en " + delim.center + " et de taille " + delim.size);
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


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        delimitation = new Bounds(transform.position,tailleMax );
        racine = new Nodes(maxProfondeur, delimitation, 1);
        renderOctree(racine);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
