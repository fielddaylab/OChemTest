# OChemTest

## Rules
- Click on an atom, hold, and drag, a transparent sphere will be centered at the center of the atom, atoms within this region (except the atom being held) can be affected by the atom being held once mouse is released.
- When an atom is first clicked on, detach all its connected neighbours
- When dragging is ended (on mouse up), find all atoms within this sphere that have bonding positions available and that are not already connected with the atom that was held (in other words, there's no path connecting the two&sup1;), sort them in descending order by priority = total mass of the chain the atom is in / distance to the atom that was held. Atoms are bonded from the highest to the lowest priority. The number of atoms that will be bonded to the atom that was held depend on how many available positions the latter has. <br/>

	

- How bonding works <br />
  A chain is snapped to the atom that was held, and the latter does not move because it's superpowered. <br />
  The closest of available bonding position at the latter is found. The forward vector of atoms' local space coords always points to the direction to their connected neighbours at bonding position 0 so that all subsequent transforms can be determined without chains of Quaternion multiplications (lots of complexity) &sup2;.  Using breadth first search, update each atom position and  rotation in the chain. For example: 
  ```c#
    //e: atom in the chain
    //this: atom that was held
    e.transform.position = this.rot 
	    * (CHBondLength/sqrt3 * pos) 
		+ this.transform.position;
	e.transform.forward = this.transform.position - e.transform.position;
	e.rot = e.transform.rotation;
  ```

## Notes

1. Drawback: this makes forming rings impossible <br />
2. Why we don't make use of Unity's transform hiearachy and set parent-child instead: <br />
	1. logically there is really no "root" in a carbon chain. We cannot just decide the first atom we interact with is the root of the chain.
	2. When an atom attracts another atom in a chain, we need to either change the latter's transform first and change other 		 atoms' transform based on that - which means we need a traversal in the transform tree. If so, essentially we will be 		treating the first atom being transformed as the "root", but it's not necessarily the root in the transform hierarchy as 		well - more complication! What we really needed was a graph of atom connections independent from transform hierarchy, 		and in fact, no transform hierarchy, as what we have now.
		
## Bugs
- When dragging a carbon to attract multiple atoms, if one of them is hydrogen, the hydrogen might return -1 (no bonding   position available) as its bonding position at this carbon even if this carbon still has positions available.

## TODO
- What Happens on Collision Enter/Exit <br />
  ```c#
    void OnCollisionEnter();
    
    void OnCollisionExit();
  ```
  - Destroy Atoms with bonds?
  - How to break colliding atoms out of the chain they are in?
- Better Move as Group
  - When to reorganize connections? <br />
    ```c#
        void DetachNeighbours();
        
    ```

