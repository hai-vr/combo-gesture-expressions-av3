## Maintenance document and technical debts

### Blend tree copies

Some assumptions:

- A blend tree asset is the instance itself.
- Original Blend trees asset must be left intact.
- Blend trees can contain other blend trees.
- It is illegal for a blend tree to have any children which contains itself, recursively.
- A blend tree may be used multiple times in a manifest.
- A blend tree may be used multiple times in another blend tree or another manifest.

Some observations:

- Blend trees cannot be copied easily.
- Object Instantiate on a blend tree will generate weird assetion error.
  - Therefore blend trees are copied manually from scratch.

To copy a blend tree:

- Find all blend trees assets of the manifest.
- Create a dictionary between that asset and a new blend tree instance.
- Copy the original blend tree into that new blend tree instance, but whenever a blend tree has a reference to another blend tree, use the dictionary above to reference the new blend tree instead.


### The issue with Animation Qualification

A qualification is metadata attached to an animation asset.
This metadata is usually whether the animation describes a eyes blinking animation, but it may also contain lipsync animation and more.

The problem is that qualification is attached to a mood set (internally called a manifest). However, multiple manifests may contain a reference to the same animation asset with different metadata.
This causes a few hurdles:
- When deduplicating states, we must deduplicate by animation asset + metadata.
- Blend trees may be used multiple times in different manifest, so deduplicating them causes a challenge; also consider the fact that blend trees may be nested, which complicates things further.

There is no straightforward way to fix this. One idea would be that manifests must contain a reference to an unique metadata reference, which would be required to be the same for all manifests of a compiler.
