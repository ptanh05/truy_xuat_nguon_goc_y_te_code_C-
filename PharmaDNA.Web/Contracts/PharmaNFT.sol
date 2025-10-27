// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "@openzeppelin/contracts/token/ERC721/extensions/ERC721URIStorage.sol";
import "@openzeppelin/contracts/access/Ownable.sol";
import "@openzeppelin/contracts/utils/Strings.sol";
import "@openzeppelin/contracts/utils/structs/EnumerableSet.sol";
import "@openzeppelin/contracts/utils/Pausable.sol";

/**
 * PharmaNFT
 * - Minimal breaking changes for existing frontend/API:
 *   - Keeps functions: roles(address), assignRole(address, Role), mintProductNFT(string),
 *     transferProductNFT(uint256,address), getProductHistory(uint256)
 * - Adds: revokeRole, batchAssignRoles, hasRole, getRole, getProductCurrentOwner,
 *   pause/unpause, role guards for admin operations
 */
contract PharmaNFT is ERC721URIStorage, Ownable, Pausable {
    using EnumerableSet for EnumerableSet.AddressSet;

    enum Role { None, Manufacturer, Distributor, Pharmacy, Admin }

    // Role management
    mapping(address => Role) public roles; // preserved public getter for FE/API compatibility
    EnumerableSet.AddressSet private allUsers;

    // Product lifecycle
    mapping(uint256 => address[]) public productHistory; // preserved getter
    uint256 public nextTokenId;

    // Events
    event RoleAssigned(address indexed user, Role role);
    event RoleRevoked(address indexed user);
    event RolesBatchAssigned(uint256 count);
    event ProductMinted(uint256 indexed tokenId, address indexed manufacturer, string uri);
    event ProductTransferred(uint256 indexed tokenId, address indexed from, address indexed to);

    // Modifiers
    modifier onlyRole(Role r) {
        require(roles[msg.sender] == r, "Invalid role");
        _;
    }

    modifier onlyTokenOwner(uint256 tokenId) {
        require(ownerOf(tokenId) == msg.sender, "Not token owner");
        _;
    }

    constructor(address initialOwner) ERC721("PharmaNFT", "PHARMA") Ownable(initialOwner) {}

    // Role management functions
    function assignRole(address user, Role role) public onlyOwner {
        require(user != address(0), "Invalid address");
        roles[user] = role;
        allUsers.add(user);
        emit RoleAssigned(user, role);
    }

    function revokeRole(address user) public onlyOwner {
        require(user != address(0), "Invalid address");
        roles[user] = Role.None;
        allUsers.remove(user);
        emit RoleRevoked(user);
    }

    function batchAssignRoles(address[] calldata users, Role[] calldata roleList) public onlyOwner {
        require(users.length == roleList.length, "Arrays length mismatch");
        for (uint256 i = 0; i < users.length; i++) {
            require(users[i] != address(0), "Invalid address");
            roles[users[i]] = roleList[i];
            if (roleList[i] != Role.None) {
                allUsers.add(users[i]);
            } else {
                allUsers.remove(users[i]);
            }
        }
        emit RolesBatchAssigned(users.length);
    }

    function hasRole(address user, Role role) public view returns (bool) {
        return roles[user] == role;
    }

    function getRole(address user) public view returns (Role) {
        return roles[user];
    }

    // Product lifecycle functions
    function mintProductNFT(string memory uri) public onlyRole(Role.Manufacturer) whenNotPaused returns (uint256) {
        uint256 tokenId = nextTokenId++;
        _safeMint(msg.sender, tokenId);
        _setTokenURI(tokenId, uri);
        
        // Initialize product history
        productHistory[tokenId].push(msg.sender);
        
        emit ProductMinted(tokenId, msg.sender, uri);
        return tokenId;
    }

    function transferProductNFT(uint256 tokenId, address to) public onlyTokenOwner(tokenId) whenNotPaused {
        require(roles[to] == Role.Distributor || roles[to] == Role.Pharmacy, "Recipient must have a role");
        require(to != address(0), "Invalid recipient");
        
        address from = ownerOf(tokenId);
        _transfer(from, to);
        
        // Update product history
        productHistory[tokenId].push(to);
        
        emit ProductTransferred(tokenId, from, to);
    }

    function getProductHistory(uint256 tokenId) public view returns (address[] memory) {
        return productHistory[tokenId];
    }

    function getProductCurrentOwner(uint256 tokenId) public view returns (address) {
        return ownerOf(tokenId);
    }

    // Admin functions
    function pause() public onlyOwner {
        _pause();
    }

    function unpause() public onlyOwner {
        _unpause();
    }

    // Override functions to add role checks
    function _beforeTokenTransfer(
        address from,
        address to,
        uint256 tokenId,
        uint256 batchSize
    ) internal override whenNotPaused {
        super._beforeTokenTransfer(from, to, tokenId, batchSize);
        
        // Only allow transfers to users with roles
        if (to != address(0)) {
            require(roles[to] != Role.None, "Recipient must have a role");
        }
    }

    // Utility functions
    function getAllUsers() public view returns (address[] memory) {
        return allUsers.values();
    }

    function getUserCount() public view returns (uint256) {
        return allUsers.length();
    }

    function supportsInterface(bytes4 interfaceId) public view override(ERC721URIStorage) returns (bool) {
        return super.supportsInterface(interfaceId);
    }
}
