# OChemTest

## Rules
- Click on an atom, hold, and drag, a transparent sphere will be centered at the center of the atom, atoms within this region (except the atom being held) can be affected by the atom being held once mouse is released.

- When dragging is ended (on mouse up), find all atoms within this sphere that have bonding positions available and that are not already connected with the atom that was held (in other words, there's no path connecting the two&sup1;), sort them in descending order by priority = total mass of the chain the atom is in / distance to the atom that was held. Atoms are bonded from the highest to the lowest priority. The number of atoms that will be bonded to the atom that was held depend on how many available positions the latter has. <br/>

  &sup1; Drawback: this makes forming rings impossible

- How bonding works <br />
  
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

