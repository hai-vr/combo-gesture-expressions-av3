## Maintenance document

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
